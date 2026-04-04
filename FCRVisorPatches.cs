using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rumi.FixCameraResolutions.Visors
{
    public static class FCRVisorPatches
    {
        private static readonly System.Reflection.FieldInfo helmetGoopField = AccessTools.Field(typeof(HUDManager), "helmetGoop");
        private static readonly Dictionary<int, bool> originalRendererStates = new Dictionary<int, bool>();
        private static readonly Dictionary<int, GameObject> overlayObjects = new Dictionary<int, GameObject>();
        private static Mesh quadMesh;

        public static bool disable
        {
            get
            {
                var visorConfig = FCRPlugin.visorConfig;
                return visorConfig != null && visorConfig.disable;
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPostfix]
        private static void PlayerControllerB_Start_Postfix(PlayerControllerB __instance)
        {
            UpdatePlayer(__instance);
        }

        [HarmonyPatch(typeof(HUDManager), "DisplaySpitOnHelmet")]
        [HarmonyPostfix]
        private static void HUDManager_DisplaySpitOnHelmet_Postfix()
        {
            SyncOverlayState(GameNetworkManager.Instance?.localPlayerController);
        }

        [HarmonyPatch(typeof(HUDManager), "Update")]
        [HarmonyPostfix]
        private static void HUDManager_Update_Postfix()
        {
            SyncOverlayState(GameNetworkManager.Instance?.localPlayerController);
        }

        public static void UpdateAllPlayer()
        {
            var players = UnityEngine.Object.FindObjectsByType<PlayerControllerB>(0);
            for (int i = 0; i < players.Length; i++)
            {
                UpdatePlayer(players[i]);
            }
        }

        public static void UpdatePlayer(PlayerControllerB player)
        {
            if (player == null || player.localVisor == null)
                return;

            var scavengerHelmet = player.localVisor.Find("ScavengerHelmet");
            if (scavengerHelmet == null)
                return;

            // Keep the original visor hierarchy alive so HUDManager's local spit-on-helmet
            // path can still drive the same object/material chain as vanilla.
            scavengerHelmet.gameObject.SetActive(true);
            UpdateHelmetRenderers(scavengerHelmet);
            EnsureOverlayObject(player);
            SyncOverlayState(player);
        }

        private static void UpdateHelmetRenderers(Transform scavengerHelmet)
        {
            Renderer[] renderers = scavengerHelmet.GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                    continue;

                int instanceId = renderer.GetInstanceID();
                if (!originalRendererStates.ContainsKey(instanceId))
                {
                    originalRendererStates[instanceId] = renderer.enabled;
                }

                renderer.enabled = disable ? false : originalRendererStates[instanceId];
            }
        }

        private static GameObject GetHelmetGoop()
        {
            if (HUDManager.Instance == null || helmetGoopField == null)
                return null;

            return helmetGoopField.GetValue(HUDManager.Instance) as GameObject;
        }

        private static void EnsureOverlayObject(PlayerControllerB player)
        {
            if (player == null || player.gameplayCamera == null)
                return;

            int playerId = player.GetInstanceID();
            if (overlayObjects.ContainsKey(playerId) && overlayObjects[playerId] != null)
            {
                UpdateOverlayTransform(player, overlayObjects[playerId].transform);
                return;
            }

            GameObject helmetGoop = GetHelmetGoop();
            Renderer sourceRenderer = helmetGoop != null ? helmetGoop.GetComponentInChildren<Renderer>(true) : null;
            if (sourceRenderer == null)
                return;

            GameObject overlayObject = new GameObject("FixCameraHelmetGoopOverlay");
            overlayObject.layer = player.gameplayCamera.gameObject.layer;
            overlayObject.transform.SetParent(player.gameplayCamera.transform, false);

            MeshFilter meshFilter = overlayObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = GetQuadMesh();

            MeshRenderer meshRenderer = overlayObject.AddComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            meshRenderer.allowOcclusionWhenDynamic = false;
            meshRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
            meshRenderer.enabled = false;

            UpdateOverlayTransform(player, overlayObject.transform);
            overlayObject.SetActive(false);
            overlayObjects[playerId] = overlayObject;
        }

        private static void SyncOverlayState(PlayerControllerB player)
        {
            if (player == null || player.gameplayCamera == null)
                return;

            EnsureOverlayObject(player);

            int playerId = player.GetInstanceID();
            if (!overlayObjects.ContainsKey(playerId))
                return;

            GameObject overlayObject = overlayObjects[playerId];
            if (overlayObject == null)
                return;

            GameObject helmetGoop = GetHelmetGoop();
            Renderer sourceRenderer = helmetGoop != null ? helmetGoop.GetComponentInChildren<Renderer>(true) : null;
            Renderer overlayRenderer = overlayObject.GetComponent<Renderer>();
            if (sourceRenderer == null || overlayRenderer == null)
                return;

            UpdateOverlayTransform(player, overlayObject.transform);
            overlayRenderer.sharedMaterials = sourceRenderer.sharedMaterials;

            bool showOverlay = disable && helmetGoop.activeInHierarchy;
            overlayObject.SetActive(showOverlay);
            overlayRenderer.enabled = showOverlay;
        }

        private static void UpdateOverlayTransform(PlayerControllerB player, Transform overlayTransform)
        {
            if (player == null || player.gameplayCamera == null || overlayTransform == null)
                return;

            Camera camera = player.gameplayCamera;
            float distance = Mathf.Max(camera.nearClipPlane + 0.015f, 0.05f);
            float height = 2f * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance;
            float width = height * camera.aspect;

            overlayTransform.localPosition = new Vector3(0f, 0f, distance);
            overlayTransform.localRotation = Quaternion.identity;
            overlayTransform.localScale = new Vector3(width * 1.02f, height * 1.02f, 1f);
        }

        private static Mesh GetQuadMesh()
        {
            if (quadMesh != null)
                return quadMesh;

            GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Quad);
            try
            {
                MeshFilter meshFilter = primitive.GetComponent<MeshFilter>();
                quadMesh = meshFilter != null ? meshFilter.sharedMesh : null;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(primitive);
            }

            return quadMesh;
        }

        internal static void Patch()
        {
            if (!disable) return;

            Debug.Log("Visor Patch...");
            try
            {
                FCRPlugin.harmony.PatchAll(typeof(FCRVisorPatches));
                Debug.Log("Visor Patched!");
            }
            catch (Exception data)
            {
                Debug.LogError(data);
                Debug.LogError("Resolution Patch Fail!");
            }
        }
    }
}

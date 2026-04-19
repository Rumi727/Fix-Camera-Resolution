using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Rumi.FixCameraResolutions.Visors
{
    public static class FCRVisorPatches
    {
        static readonly System.Reflection.FieldInfo helmetGoopField = AccessTools.Field(typeof(HUDManager), "helmetGoop");
        static readonly Dictionary<int, bool> originalRendererStates = new Dictionary<int, bool>();
        static readonly Dictionary<int, GameObject> overlayObjects = new Dictionary<int, GameObject>();
        static Mesh? quadMesh;

        public static bool disable => FCRPlugin.visorConfig?.disable ?? FCRVisorConfig.dDisable;

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPostfix]
        static void PlayerControllerB_Start_Postfix(PlayerControllerB __instance) => UpdatePlayer(__instance);

        [HarmonyPatch(typeof(HUDManager), "DisplaySpitOnHelmet")]
        [HarmonyPostfix]
        static void HUDManager_DisplaySpitOnHelmet_Postfix() => SyncOverlayState(GameNetworkManager.Instance?.localPlayerController);

        [HarmonyPatch(typeof(HUDManager), "Update")]
        [HarmonyPostfix]
        static void HUDManager_Update_Postfix() => SyncOverlayState(GameNetworkManager.Instance?.localPlayerController);

        public static void UpdateAllPlayer()
        {
            PlayerControllerB[] players = Object.FindObjectsByType<PlayerControllerB>(FindObjectsSortMode.None);
            for (int i = 0; i < players.Length; i++)
                UpdatePlayer(players[i]);
        }

        public static void UpdatePlayer(PlayerControllerB? player)
        {
            if (player?.localVisor == null)
                return;

            Transform? scavengerHelmet = player.localVisor.Find("ScavengerHelmet");
            if (scavengerHelmet == null)
                return;

            // Keep the vanilla visor/goop chain alive, but hide the visor renderers.
            scavengerHelmet.gameObject.SetActive(true);
            UpdateHelmetRenderers(scavengerHelmet);
            EnsureOverlayObject(player);
            SyncOverlayState(player);
        }

        static void UpdateHelmetRenderers(Transform scavengerHelmet)
        {
            Renderer[] renderers = scavengerHelmet.GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                int instanceId = renderer.GetInstanceID();

                if (!originalRendererStates.ContainsKey(instanceId))
                    originalRendererStates[instanceId] = renderer.enabled;

                renderer.enabled = disable ? false : originalRendererStates[instanceId];
            }
        }

        static GameObject? GetHelmetGoop()
        {
            if (HUDManager.Instance == null || helmetGoopField == null)
                return null;

            return helmetGoopField.GetValue(HUDManager.Instance) as GameObject;
        }

        static void EnsureOverlayObject(PlayerControllerB? player)
        {
            if (player?.gameplayCamera == null)
                return;

            int playerId = player.GetInstanceID();
            if (overlayObjects.TryGetValue(playerId, out GameObject? overlayObject) && overlayObject != null)
            {
                UpdateOverlayTransform(player, overlayObject.transform);
                return;
            }

            GameObject? helmetGoop = GetHelmetGoop();
            Renderer? sourceRenderer = helmetGoop != null ? helmetGoop.GetComponentInChildren<Renderer>(true) : null;
            if (sourceRenderer == null)
                return;

            overlayObject = new GameObject("FixCameraHelmetGoopOverlay");
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

        static void SyncOverlayState(PlayerControllerB? player)
        {
            if (player?.gameplayCamera == null)
                return;

            EnsureOverlayObject(player);

            int playerId = player.GetInstanceID();
            if (!overlayObjects.TryGetValue(playerId, out GameObject? overlayObject) || overlayObject == null)
                return;

            GameObject? helmetGoop = GetHelmetGoop();
            Renderer? sourceRenderer = helmetGoop != null ? helmetGoop.GetComponentInChildren<Renderer>(true) : null;
            Renderer? overlayRenderer = overlayObject.GetComponent<Renderer>();
            if (sourceRenderer == null || overlayRenderer == null)
                return;

            UpdateOverlayTransform(player, overlayObject.transform);
            overlayRenderer.sharedMaterials = sourceRenderer.sharedMaterials;

            bool showOverlay = disable && helmetGoop != null && helmetGoop.activeInHierarchy;
            overlayObject.SetActive(showOverlay);
            overlayRenderer.enabled = showOverlay;
        }

        static void UpdateOverlayTransform(PlayerControllerB player, Transform overlayTransform)
        {
            Camera camera = player.gameplayCamera;
            float distance = Mathf.Max(camera.nearClipPlane + 0.015f, 0.05f);
            float height = 2f * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance;
            float width = height * camera.aspect;

            overlayTransform.localPosition = new Vector3(0f, 0f, distance);
            overlayTransform.localRotation = Quaternion.identity;
            overlayTransform.localScale = new Vector3(width * 1.02f, height * 1.02f, 1f);
        }

        static Mesh? GetQuadMesh()
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
                Object.DestroyImmediate(primitive);
            }

            return quadMesh;
        }

        internal static void Patch()
        {
            if (!disable)
                return;

            Debug.Log("Visor Patch...");

            try
            {
                FCRPlugin.harmony.PatchAll(typeof(FCRVisorPatches));
                Debug.Log("Visor Patched!");
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("Resolution Patch Fail!");
            }
        }
    }
}

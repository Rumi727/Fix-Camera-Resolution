using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Rumi.FixCameraResolutions.Fogs
{
    public static class FCRFogPatches
    {
        [HarmonyPatch(typeof(Volume))]
        static class OnEnablePatch
        {
            [HarmonyPatch("OnEnable"), HarmonyPostfix]
            static void OnEnable(Volume __instance) => UpdateVolume(__instance);
        }

        [HarmonyPatch(typeof(Volume))]
        static class UpdatePatch
        {
            [HarmonyPatch("Update"), HarmonyPostfix] static void Update(Volume __instance) => UpdateVolume(__instance);
        }

        [HarmonyPatch(typeof(HDAdditionalCameraData))]
        static class HDAdditionalCameraDataPatch
        {
            [HarmonyPatch("Awake"), HarmonyPostfix] static void Awake(HDAdditionalCameraData __instance) => UpdateHDCameraData(__instance);
        }

        public static FogMode fogMode => FCRPlugin.fogConfig?.fogMode ?? FCRFogConfig.dFogMode;

        public static void UpdateAll()
        {
            UpdateAllVolume();
            UpdateAllHDCameraData();
        }

        public static void UpdateAllVolume()
        {
            Volume[] volumes = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
            for (int i = 0; i < volumes.Length; i++)
                UpdateVolume(volumes[i]);
        }

        public static void UpdateVolume(Volume volume)
        {
            if (volume.sharedProfile == null)
                return;

            if (volume.sharedProfile.TryGet(out Fog fog))
                fog.active = fogMode != FogMode.Disable && fogMode != FogMode.ForceDisable;
        }

        public static void UpdateAllHDCameraData()
        {
            HDAdditionalCameraData[] cameraDatas = Object.FindObjectsByType<HDAdditionalCameraData>(FindObjectsSortMode.None);
            for (int i = 0; i < cameraDatas.Length; i++)
                UpdateHDCameraData(cameraDatas[i]);
        }

        public static void UpdateHDCameraData(HDAdditionalCameraData cameraData)
        {
            /* 
             * API 문서 겁나 뒤져보다가 발견한 설정인데
             * 이게 왜 되는지는 잘 모르겠다
             */
            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.Volumetrics] = fogMode != FogMode.Vanilla;
            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Volumetrics, fogMode == FogMode.Vanilla);
        }

        internal static void Patch()
        {
            if (fogMode == FogMode.Vanilla)
                return;

            Debug.Log("Fog Patch...");

            try
            {
                if (fogMode != FogMode.Vanilla)
                    FCRPlugin.harmony.PatchAll(typeof(HDAdditionalCameraDataPatch));

                if (fogMode == FogMode.Disable || fogMode == FogMode.ForceDisable)
                {
                    FCRPlugin.harmony.PatchAll(typeof(OnEnablePatch));
                    if (fogMode == FogMode.ForceDisable)
                        FCRPlugin.harmony.PatchAll(typeof(UpdatePatch));
                }

                Debug.Log("Fog Patched!");
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("Fog Patch Fail!");
            }
        }
    }
}

using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Rumi.FixCameraResolutions.Fogs
{
    public static class FCRHDRPPatches
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

        public static AntialiasingMode antialiasingMode => FCRPlugin.hdrpConfig?.antialiasingMode ?? FCRHDRPConfig.dAntialiasingMode;
        public static HDRPMode bloomMode => FCRPlugin.hdrpConfig?.bloomMode ?? FCRHDRPConfig.dBloomMode;
        public static FogMode fogMode => FCRPlugin.hdrpConfig?.fogMode ?? FCRHDRPConfig.dFogMode;
        public static HDRPMode shadowMode => FCRPlugin.hdrpConfig?.shadowMode ?? FCRHDRPConfig.dShadowMode;
        public static HDRPMode postProcessingMode => FCRPlugin.hdrpConfig?.postProcessingMode ?? FCRHDRPConfig.dPostProcessingMode;
        public static HDRPMode vignetteMode => FCRPlugin.hdrpConfig?.vignetteMode ?? FCRHDRPConfig.dVignetteMode;

        public static void UpdateAll()
        {
            UpdateAllVolume();
            UpdateAllHDCameraData();
        }

        public static void UpdateAllVolume()
        {
            Volume[] volumes = Object.FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
            HDAdditionalCameraData[] cameraDatas = Object.FindObjectsByType<HDAdditionalCameraData>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < cameraDatas.Length; i++)
                UpdateHDCameraData(cameraDatas[i]);
        }

        public static void UpdateHDCameraData(HDAdditionalCameraData cameraData)
        {
            /*
             * 관전시에 설정 적용 안되는 버그 수정
             * ...이거 왜 됨
             */
            if (cameraData.gameObject.name == "SpectateCamera")
                cameraData.customRenderingSettings = !IsVanillaMode();

            if (cameraData.gameObject.name != "UICamera")
            {
                cameraData.antialiasing = (HDAdditionalCameraData.AntialiasingMode)antialiasingMode;

                cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Antialiasing] = antialiasingMode != AntialiasingMode.None;
                cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Antialiasing, antialiasingMode != AntialiasingMode.None);
            }

            /* 
             * API 문서 겁나 뒤져보다가 발견한 설정인데
             * 이게 왜 되는지는 잘 모르겠다
             */

            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Bloom] = bloomMode != HDRPMode.Vanilla;
            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Bloom, bloomMode == HDRPMode.Vanilla);

            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Volumetrics] = fogMode != FogMode.Vanilla;
            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Volumetrics, fogMode == FogMode.Vanilla);

            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ShadowMaps] = shadowMode != HDRPMode.Vanilla;
            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ShadowMaps, shadowMode == HDRPMode.Vanilla);

            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.CustomPass] = postProcessingMode != HDRPMode.Vanilla;
            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.CustomPass, postProcessingMode == HDRPMode.Vanilla);

            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Vignette] = vignetteMode != HDRPMode.Vanilla;
            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Vignette, vignetteMode == HDRPMode.Vanilla);
        }

        static bool IsVanillaMode() => antialiasingMode == AntialiasingMode.None && bloomMode == HDRPMode.Vanilla && fogMode == FogMode.Vanilla && shadowMode == HDRPMode.Vanilla && postProcessingMode == HDRPMode.Vanilla && vignetteMode == HDRPMode.Vanilla;

        internal static void Patch()
        {
            if (IsVanillaMode())
                return;

            Debug.Log("Fog Patch...");

            try
            {
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

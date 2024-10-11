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

        [HarmonyPatch(typeof(Fog))]
        static class FogPatch
        {
            [HarmonyPatch("volumetricFogBudget", MethodType.Getter), HarmonyPostfix] static void volumetricFogBudget(ref float __result) => __result = 0.0f;
            [HarmonyPatch("resolutionDepthRatio", MethodType.Getter), HarmonyPostfix] static void resolutionDepthRatio(ref float __result) => __result = 0.0f;
        }

        public static FogMode fogMode => FCRPlugin.fogConfig?.fogMode ?? FCRFogConfig.dFogMode;

        public static void UpdateAllVolume()
        {
            Volume[] volumes = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
            for (int i = 0; i < volumes.Length; i++)
            {
                Volume volume = volumes[i];
                UpdateVolume(volume);
            }
        }

        public static void UpdateVolume(Volume volume)
        {
            if (volume.sharedProfile == null)
                return;

            if (volume.sharedProfile.TryGet(out Fog fog))
                fog.active = fogMode != FogMode.Disable && fogMode != FogMode.ForceDisable;
        }

        internal static void Patch()
        {
            if (fogMode == FogMode.Vanilla)
                return;

            Debug.Log("Fog Patch...");

            try
            {
                if (fogMode == FogMode.Hide)
                    FCRPlugin.harmony.PatchAll(typeof(FogPatch));
                else if (fogMode == FogMode.Disable || fogMode == FogMode.ForceDisable)
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

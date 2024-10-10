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
            [HarmonyPostfix] static void OnEnable(Volume __instance) => UpdateVolume(__instance);
        }

        [HarmonyPatch(typeof(Volume))]
        static class UpdatePatch
        {
            [HarmonyPostfix] static void Update(Volume __instance) => UpdateVolume(__instance);
        }

        public static bool disable => FCRPlugin.fogConfig?.disable ?? FCRFogConfig.dDisable;
        public static bool alwaysUpdate => FCRPlugin.fogConfig?.alwaysUpdate ?? FCRFogConfig.dAlwaysUpdate;

        public static void UpdateAllVolume()
        {
            Volume[] volumes = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
            for (int i = 0; i < volumes.Length; i++)
                UpdateVolume(volumes[i]);
        }

        public static void UpdateVolume(Volume volume)
        {
            for (int i = 0; i < volume.profile.components.Count; i++)
            {
                VolumeComponent component = volume.profile.components[i];
                if (component != null && component is Fog fog)
                    fog.enabled.value = !disable;
            }
        }

        internal static void Patch()
        {
            if (!disable)
                return;

            Debug.Log("Fog Patch...");

            try
            {
                if (disable)
                {
                    FCRPlugin.harmony.PatchAll(typeof(OnEnablePatch));
                    if (alwaysUpdate)
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

using HarmonyLib;
using System.Collections;
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
            static void OnEnable(Volume __instance) => __instance.StartCoroutine(UpdateVolume(__instance));
        }

        [HarmonyPatch(typeof(Volume))]
        static class UpdatePatch
        {
            [HarmonyPatch("Update"), HarmonyPostfix] static void Update(Volume __instance) => UpdateVolume(__instance);
        }

        public static bool disable => FCRPlugin.fogConfig?.disable ?? FCRFogConfig.dDisable;
        public static bool alwaysUpdate => FCRPlugin.fogConfig?.alwaysUpdate ?? FCRFogConfig.dAlwaysUpdate;

        public static void UpdateAllVolume()
        {
            Volume[] volumes = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
            for (int i = 0; i < volumes.Length; i++)
            {
                Volume volume = volumes[i];
                volume.StartCoroutine(UpdateVolume(volume));
            }
        }

        public static IEnumerator UpdateVolume(Volume volume)
        {
            //profile이 생성될 때까지 대기
            yield return new WaitForEndOfFrame();

            if (volume.profile == null)
                yield break;

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

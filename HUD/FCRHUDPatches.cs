using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Rumi.FixCameraResolutions.HUD
{
    public static class FCRHUDPatches
    {
        public static bool fixedAspectRatio => FCRPlugin.hudConfig?.fixedAspectRatio ?? FCRHUDConfig.dFixedAspectRatio;
        public static bool checkResolutionEveryFrame => FCRPlugin.hudConfig?.checkResolutionEveryFrame ?? FCRHUDConfig.dCheckResolutionEveryFrame;



        [HarmonyPatch(typeof(HUDManager))]
        static class StartPatch
        {
            [HarmonyPatch("Start"), HarmonyPostfix]
            static void Start(HUDManager __instance) => UpdateHUDManager(__instance);
        }


        [HarmonyPatch(typeof(HUDManager))]
        static class UpdatePath
        {
            static int lastScreenWidth;
            static int lastScreenHeight;

            [HarmonyPatch("Update"), HarmonyPostfix]
            static void Update(HUDManager __instance)
            {
                if (lastScreenWidth != Screen.width || lastScreenHeight != Screen.height)
                {
                    UpdateHUDManager(__instance);

                    lastScreenWidth = Screen.width;
                    lastScreenHeight = Screen.height;
                }
            }
        }

        public static void UpdateHUDManager(HUDManager? instance)
        {
            if (instance == null)
                return;

            AspectRatioFitter? hudContainer = instance.HUDContainer.GetComponent<AspectRatioFitter>();
            if (hudContainer != null)
                UpdateFitter(hudContainer);

            Transform? hudContainerParent = instance.HUDContainer.transform.parent;
            Transform? panelTransform = hudContainerParent != null ? hudContainerParent.Find("Panel") : null;

            if (panelTransform != null)
            {
                AspectRatioFitter? panel = panelTransform.GetComponent<AspectRatioFitter>();
                if (panel != null)
                    UpdateFitter(panel);
            }
        }

        public static void UpdateFitter(AspectRatioFitter aspectRatioFitter)
        {
            if (fixedAspectRatio)
                aspectRatioFitter.aspectRatio = 1.76f;
            else
                aspectRatioFitter.aspectRatio = (float)Screen.width / Screen.height;
        }

        internal static void Patch()
        {
            if (fixedAspectRatio)
                return;

            Debug.Log("HUD Patch...");

            try
            {
                FCRPlugin.harmony.PatchAll(typeof(StartPatch));
                if (checkResolutionEveryFrame)
                    FCRPlugin.harmony.PatchAll(typeof(UpdatePath));

                Debug.Log("HUD Patched!");
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("Resolution Patch Fail!");
            }
        }
    }
}

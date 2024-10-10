using HarmonyLib;
using UnityEngine;

namespace Rumi.FixCameraResolutions
{
    public static class FCRResPatches
    {
        public static int width
        {
            get
            {
                if (FCRPlugin.resConfig?.autoSize ?? FCRResConfig.dAutoSize)
                    return Screen.width;
                else
                    return FCRPlugin.resConfig?.width ?? FCRResConfig.dWidth;
            }
        }

        public static int height
        {
            get
            {
                if (FCRPlugin.resConfig?.autoSize ?? FCRResConfig.dAutoSize)
                    return Screen.height;
                else
                    return FCRPlugin.resConfig?.height ?? FCRResConfig.dHeight;
            }
        }

        public static int? orgWidth { get; private set; }
        public static int? orgHeight { get; private set; }



        [HarmonyPatch(typeof(Terminal), "Start")]
        [HarmonyPostfix]
        static void Terminal_Start_Postfix(Terminal __instance)
        {
            orgWidth ??= __instance.playerScreenTex.width;
            orgHeight ??= __instance.playerScreenTex.height;

            RenderTexturePatch(__instance.playerScreenTex);
            RenderTexturePatch(__instance.playerScreenTexHighRes);
        }

        //스캔 노드 위치 버그 수정
        [HarmonyPatch(typeof(HUDManager), "UpdateScanNodes")]
        [HarmonyPostfix]
        static void HUDManager_UpdateScanNodes_Postfix(HUDManager __instance)
        {
            for (int i = 0; i < __instance.scanElements.Length; i++)
            {
                RectTransform scanElement = __instance.scanElements[i];

                scanElement.anchoredPosition += new Vector2(439.48f, 244.8f);
                scanElement.anchoredPosition = scanElement.anchoredPosition.Multiply((orgWidth / (float)width) ?? 1f, (orgHeight / (float)height) ?? 1);
                scanElement.anchoredPosition -= new Vector2(439.48f, 244.8f);
            }
        }

        public static void AllTerminalPatch()
        {
            Terminal[] terminal = Object.FindObjectsByType<Terminal>(FindObjectsSortMode.None);
            for (int i = 0; i < terminal.Length; i++)
            {
                RenderTexturePatch(terminal[i].playerScreenTex);
                RenderTexturePatch(terminal[i].playerScreenTexHighRes);
            }
        }

        public static void RenderTexturePatch(RenderTexture renderTexture)
        {
            int targetWidth = width;
            int targetHeight = height;

            renderTexture.Release();

            renderTexture.width = targetWidth;
            renderTexture.height = targetHeight;

            Debug.Log($"Changed the size of the render texture to {targetWidth}x{targetHeight}");
        }
    }
}

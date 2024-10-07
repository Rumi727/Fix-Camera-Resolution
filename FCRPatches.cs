using HarmonyLib;
using UnityEngine;

namespace Rumi.FixCameraResolutions
{
    public class FCRPatches
    {
        public static int width
        {
            get
            {
                if (FCRPlugin.config?.autoSize ?? FCRConfig.dAutoSize)
                    return Screen.width;
                else
                    return FCRPlugin.config?.width ?? FCRConfig.dWidth;
            }
        }

        public static int height
        {
            get
            {
                if (FCRPlugin.config?.autoSize ?? FCRConfig.dAutoSize)
                    return Screen.height;
                else
                    return FCRPlugin.config?.height ?? FCRConfig.dHeight;
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
            int targetWidth;
            int targetHeight;

            if (FCRPlugin.config?.autoSize ?? FCRConfig.dAutoSize)
            {
                targetWidth = Screen.width;
                targetHeight = Screen.height;
            }
            else
            {
                targetWidth = FCRPlugin.config?.width ?? FCRConfig.dWidth;
                targetHeight = FCRPlugin.config?.height ?? FCRConfig.dHeight;
            }

            renderTexture.Release();

            renderTexture.width = targetWidth;
            renderTexture.height = targetHeight;

            FCRPlugin.logger.LogInfo($"Changed the size of the render texture to {targetWidth}x{targetHeight}");
        }
    }
}

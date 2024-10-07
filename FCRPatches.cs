using HarmonyLib;
using UnityEngine;

namespace Rumi.FixCameraResolutions
{
    public class FCRPatches
    {
        [HarmonyPatch(typeof(Terminal), "Start")]
        [HarmonyPostfix]
        static void Terminal_Start_Postfix(Terminal __instance)
        {
            RenderTexturePatch(__instance.playerScreenTex);
            RenderTexturePatch(__instance.playerScreenTexHighRes);
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

            FCRPlugin.logger?.LogInfo($"Changed the size of the render texture to {targetWidth}x{targetHeight}");
        }
    }
}

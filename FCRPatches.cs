using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace Rumi.FixCameraResolutions
{
    public class FCRPatches
    {
        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPostfix]
        static void PlayerControllerB_Start_Postfix(PlayerControllerB __instance) => CameraPatch(__instance.gameplayCamera);

        public static void AllCameraPatch()
        {
            PlayerControllerB[] players = Object.FindObjectsByType<PlayerControllerB>(FindObjectsSortMode.None);
            for (int i = 0; i < players.Length; i++)
                CameraPatch(players[i].gameplayCamera);
        }

        public static void CameraPatch(Camera camera)
        {
            RenderTexture? targetTexture = camera.targetTexture;
            if (targetTexture == null)
                return;

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

            camera.targetTexture.Release();

            camera.targetTexture.width = targetWidth;
            camera.targetTexture.height = targetHeight;

            FCRPlugin.logger?.LogInfo($"Changed the size of the render texture to {targetWidth}x{targetHeight}");
        }
    }
}

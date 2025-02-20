using GameNetcodeStuff;
using HarmonyLib;
using Rumi.FixCameraResolutions.HUD;
using UnityEngine;

namespace Rumi.FixCameraResolutions.Resolutions
{
    public static class FCRResPatches
    {
        public static bool enable => FCRPlugin.resConfig?.enable ?? FCRResConfig.dEnable;

        public static bool autoSize => FCRPlugin.resConfig?.autoSize ?? FCRResConfig.dAutoSize;
        public static bool checkResolutionEveryFrame => FCRPlugin.hudConfig?.checkResolutionEveryFrame ?? FCRHUDConfig.dCheckResolutionEveryFrame;

        /// <summary>
        /// orgWidth
        /// </summary>
        public static int width
        {
            get
            {
                if (enable)
                {
                    if (autoSize)
                        return Screen.width;
                    else
                        return FCRPlugin.resConfig?.width ?? FCRResConfig.dWidth;
                }
                else
                    return orgWidth;
            }
        }

        public static int height
        {
            get
            {
                if (enable)
                {
                    if (autoSize)
                        return Screen.height;
                    else
                        return FCRPlugin.resConfig?.height ?? FCRResConfig.dHeight;
                }
                else
                    return orgHeight;
            }
        }

        public const int orgWidth = 860;
        public const int orgHeight = 520;

        static class ResPatch
        {
            [HarmonyPatch(typeof(Terminal), "Start")]
            [HarmonyPostfix]
            static void Terminal_Start_Postfix(Terminal __instance)
            {
                UpdateRenderTexture(__instance.playerScreenTex);
                UpdateRenderTexture(__instance.playerScreenTexHighRes);
            }

            //스캔 노드 위치 버그 수정
            [HarmonyPatch(typeof(HUDManager), "UpdateScanNodes")]
            [HarmonyPostfix]
            static void HUDManager_UpdateScanNodes_Postfix(HUDManager __instance)
            {
                if (!enable)
                    return;

                for (int i = 0; i < __instance.scanElements.Length; i++)
                {
                    RectTransform scanElement = __instance.scanElements[i];

                    scanElement.anchoredPosition += new Vector2(439.48f, 244.8f);
                    scanElement.anchoredPosition = scanElement.anchoredPosition.Multiply((float)orgWidth / width, (float)orgHeight / height);
                    scanElement.anchoredPosition -= new Vector2(439.48f, 244.8f);
                }
            }
        }

        [HarmonyPatch(typeof(HUDManager))]
        static class UpdatePath
        {
            static int lastScreenWidth;
            static int lastScreenHeight;
            [HarmonyPatch("Update"), HarmonyPostfix]
            static void Update()
            {
                if (lastScreenWidth != Screen.width || lastScreenHeight != Screen.height)
                {
                    UpdateAll();

                    lastScreenWidth = Screen.width;
                    lastScreenHeight = Screen.height;
                }
            }
        }

        public static void UpdateAll()
        {
            UpdateAllTerminal();
            UpdateAllPlayerCamera();
        }

        public static void UpdateAllTerminal()
        {
            Terminal[] terminals = Object.FindObjectsByType<Terminal>(FindObjectsSortMode.None);
            for (int i = 0; i < terminals.Length; i++)
            {
                Terminal terminal = terminals[i];

                UpdateRenderTexture(terminal.playerScreenTex);
                UpdateRenderTexture(terminal.playerScreenTexHighRes);
            }
        }

        public static void UpdateRenderTexture(RenderTexture renderTexture)
        {
            renderTexture.Release();

            renderTexture.width = width;
            renderTexture.height = height;

            //Debug.Log($"Changed the size of the render texture to {renderTexture.width}x{renderTexture.height}");
        }

        public static void UpdateAllPlayerCamera()
        {
            PlayerControllerB[] players = Object.FindObjectsByType<PlayerControllerB>(FindObjectsSortMode.None);
            for (int i = 0; i < players.Length; i++)
                UpdateCamera(players[i].gameplayCamera);
        }

        public static void UpdateCamera(Camera camera) => camera.ResetAspect();

        internal static void Patch()
        {
            if (!enable)
                return;

            Debug.Log("Resolution Patch...");

            try
            {
                FCRPlugin.harmony.PatchAll(typeof(ResPatch));
                if (checkResolutionEveryFrame)
                    FCRPlugin.harmony.PatchAll(typeof(UpdatePath));

                Debug.Log("Resolution Patched!");
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("Resolution Patch Fail!");
            }
        }
    }
}

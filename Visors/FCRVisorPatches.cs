using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace Rumi.FixCameraResolutions.Visors
{
    public static class FCRVisorPatches
    {
        public static bool disable => FCRPlugin.visorConfig?.disable ?? FCRVisorConfig.dDisable;

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPostfix]
        static void PlayerControllerB_Start_Postfix(PlayerControllerB __instance) => UpdatePlayer(__instance);

        public static void UpdateAllPlayer()
        {
            PlayerControllerB[] players = Object.FindObjectsByType<PlayerControllerB>(FindObjectsSortMode.None);
            for (int i = 0; i < players.Length; i++)
                UpdatePlayer(players[i]);
        }

        public static void UpdatePlayer(PlayerControllerB player)
        {
            if (player.localVisor != null)
                player.localVisor.gameObject.SetActive(!disable);
        }

        internal static void Patch()
        {
            if (!disable)
                return;

            Debug.Log("Visor Patch...");

            try
            {
                FCRPlugin.harmony.PatchAll(typeof(FCRVisorPatches));
                Debug.Log("Visor Patched!");
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("Resolution Patch Fail!");
            }
        }
    }
}

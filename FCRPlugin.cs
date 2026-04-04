using System;
using System.Collections;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Rumi.FixCameraResolutions.Fogs;
using Rumi.FixCameraResolutions.HUD;
using Rumi.FixCameraResolutions.Resolutions;
using Rumi.FixCameraResolutions.Visors;

namespace Rumi.FixCameraResolutions
{
    [BepInPlugin("Rumi.FixCameraResolutions", "FixCameraResolutions", "1.4.0")]
    public sealed class FCRPlugin : BaseUnityPlugin
    {
        internal static FCRPlugin instance { get; private set; } = null;
        internal static ManualLogSource logger { get; private set; } = null;

        public static FCRResConfig resConfig { get; private set; }
        public static FCRHUDConfig hudConfig { get; private set; }
        public static FCRHDRPConfig hdrpConfig { get; private set; }
        public static FCRVisorConfig visorConfig { get; private set; }

        internal static Harmony harmony { get; } = new Harmony("Rumi.FixCameraResolutions");

        private void Awake()
        {
            logger = base.Logger;
            instance = this;
            Debug.Log("Start loading plugin...");
            resConfig = FCRResConfig.Create(base.Config);
            hudConfig = FCRHUDConfig.Create(base.Config);
            hdrpConfig = FCRHDRPConfig.Create(base.Config);
            visorConfig = FCRVisorConfig.Create(base.Config);

            if (Type.GetType("LethalConfig.LethalConfigManager, LethalConfig, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false) != null)
            {
                try
                {
                    Debug.Log("Lethal Config Registration");
                    if (resConfig != null) FCRResConfig.LethalConfig.Patch(resConfig);
                    if (hudConfig != null) FCRHUDConfig.LethalConfig.Patch(hudConfig);
                    if (hdrpConfig != null) FCRHDRPConfig.LethalConfig.Patch(hdrpConfig);
                    if (visorConfig != null) FCRVisorConfig.LethalConfig.Patch(visorConfig);
                }
                catch (Exception data)
                {
                    Debug.LogError(data);
                    Debug.LogWarning("Lethal Config Registration Fail!");
                }
            }

            base.Config.Save();
            Patch();
            Debug.Log("Plugin FixCameraResolutions is loaded!");
        }

        public static void Repatch()
        {
            if (isRepatchedToCurrentFrame) return;
            if (instance != null)
            {
                isRepatchedToCurrentFrame = true;
                instance.StartCoroutine(RepatchedToCurrentFrame());
            }
            FCRResPatches.UpdateAll();
            FCRHUDPatches.UpdateHUDManager(HUDManager.Instance);
            // FCRHDRPPatches.UpdateAll(); // 暂时注释，因为 API 变更
            FCRVisorPatches.UpdateAllPlayer();
            Unpatch();
            Patch();
        }

        private static IEnumerator RepatchedToCurrentFrame()
        {
            yield return null;
            isRepatchedToCurrentFrame = false;
        }

        private static void Patch()
        {
            FCRResPatches.Patch();
            FCRHUDPatches.Patch();
            // FCRHDRPPatches.Patch(); // 暂时注释
            FCRVisorPatches.Patch();
        }

        private static void Unpatch()
        {
            Debug.Log("Unpatch...");
            try
            {
                harmony.UnpatchSelf();
            }
            catch (Exception data)
            {
                Debug.LogError(data);
                Debug.LogError("Unpatch Fail!");
            }
            Debug.Log("Unpatched!");
        }

        public const string modGuid = "Rumi.FixCameraResolutions";
        public const string modName = "FixCameraResolutions";
        public const string modVersion = "1.4.0";

        private static bool isRepatchedToCurrentFrame = false;
    }
}
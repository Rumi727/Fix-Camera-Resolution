﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Rumi.FixCameraResolutions.Fogs;
using Rumi.FixCameraResolutions.Resolutions;
using Rumi.FixCameraResolutions.Visors;
using System;

namespace Rumi.FixCameraResolutions
{
    [BepInPlugin(modGuid, modName, modVersion)]
    public sealed class FCRPlugin : BaseUnityPlugin
    {
        public const string modGuid = "Rumi.FixCameraResolutions";
        public const string modName = "FixCameraResolutions";
        public const string modVersion = "1.3.4";

        internal static ManualLogSource? logger { get; private set; } = null;

        public static FCRResConfig? resConfig { get; private set; }
        public static FCRHDRPConfig? hdrpConfig { get; private set; }
        public static FCRVisorConfig? visorConfig { get; private set; }

        internal static Harmony harmony { get; } = new Harmony(modGuid);

        void Awake()
        {
            logger = Logger;

            Debug.Log("Start loading plugin...");

            resConfig = FCRResConfig.Create(Config);
            hdrpConfig = FCRHDRPConfig.Create(Config);
            visorConfig = FCRVisorConfig.Create(Config);

            if (Type.GetType("LethalConfig.LethalConfigManager, LethalConfig, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false) != null)
            {
                try
                {
                    Debug.Log("Lethal Config Registration");

                    if (resConfig != null)
                        FCRResConfig.LethalConfig.Patch(resConfig);

                    if (hdrpConfig != null)
                        FCRHDRPConfig.LethalConfig.Patch(hdrpConfig);

                    if (visorConfig != null)
                        FCRVisorConfig.LethalConfig.Patch(visorConfig);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    Debug.LogWarning("Lethal Config Registration Fail!");
                }
            }

            Config.Save();

            Patch();

            Debug.Log($"Plugin {modName} is loaded!");
        }

        public static void Repatch()
        {
            FCRResPatches.UpdateAll();
            FCRHDRPPatches.UpdateAll();
            FCRVisorPatches.UpdateAllPlayer();

            Unpatch();
            Patch();
        }

        static void Patch()
        {
            FCRResPatches.Patch();
            FCRHDRPPatches.Patch();
            FCRVisorPatches.Patch();
        }

        static void Unpatch()
        {
            Debug.Log("Unpatch...");

            try
            {
                harmony.UnpatchSelf();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("Unpatch Fail!");
            }

            Debug.Log("Unpatched!");
        }
    }
}

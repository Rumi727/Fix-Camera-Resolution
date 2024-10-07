using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;

namespace Rumi.FixCameraResolutions
{
    [BepInPlugin(modGuid, modName, modVersion)]
    public sealed class FCRPlugin : BaseUnityPlugin
    {
        public const string modGuid = "Rumi.FixCameraResolutions";
        public const string modName = "FixCameraResolutions";
        public const string modVersion = "1.0.2";

        internal static ManualLogSource logger { get; private set; } = null!;
        public static FCRConfig? config { get; private set; }

        public static Harmony harmony { get; } = new Harmony(modGuid);

        void Awake()
        {
            logger = Logger;

            logger.LogInfo("Start loading plugin...");

            logger.LogInfo("Config Loading...");

            try
            {
                config = new FCRConfig(Config);
            }
            catch (Exception e)
            {
                config = null;

                logger.LogError(e);
                logger.LogWarning($"Failed to load config file\nSettings will be loaded with defaults!");
            }

            logger.LogInfo("Patch...");

            try
            {
                harmony.PatchAll(typeof(FCRPatches));
            }
            catch (Exception e)
            {
                logger.LogError(e);
                logger.LogError("Patch Fail!");
            }

            logger.LogInfo($"Plugin {modName} is loaded!");
        }
    }
}

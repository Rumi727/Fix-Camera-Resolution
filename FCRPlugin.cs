using BepInEx;
using BepInEx.Logging;
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

        internal static ManualLogSource? logger { get; private set; } = null;

        public static FCRResConfig? resConfig { get; private set; }

        public static Harmony harmony { get; } = new Harmony(modGuid);

        void Awake()
        {
            logger = Logger;

            Debug.Log("Start loading plugin...");

            Debug.Log("Config Loading...");

            try
            {
                resConfig = new FCRResConfig(Config);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogWarning($"Failed to load config file\nSettings will be loaded with defaults!");
            }

            Debug.Log("Resolution Patch...");

            try
            {
                harmony.PatchAll(typeof(FCRResPatches));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("Resolution Patch Fail!");
            }

            Debug.Log($"Plugin {modName} is loaded!");
        }



        #region Obsolete
        [Obsolete("Deprecated class name! Please use FCRResConfig", true)] public static FCRConfig config => throw new NotImplementedException();
        #endregion
    }
}

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Rumi.FixCameraResolutions.Fogs;
using Rumi.FixCameraResolutions.Resolutions;

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
        public static FCRFogConfig? fogConfig { get; private set; }

        internal static Harmony harmony { get; } = new Harmony(modGuid);

        void Awake()
        {
            logger = Logger;

            Debug.Log("Start loading plugin...");

            resConfig = FCRResConfig.Create(Config);
            fogConfig = FCRFogConfig.Create(Config);

            Patch();

            Debug.Log($"Plugin {modName} is loaded!");
        }

        public static void Repatch()
        {
            FCRResPatches.UpdateAllTerminal();
            FCRFogPatches.UpdateAllVolume();

            Unpatch();
            Patch();
        }

        static void Patch()
        {
            FCRResPatches.Patch();
            FCRFogPatches.Patch();
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



        #region Obsolete
        [System.Obsolete("Deprecated class name! Please use FCRResConfig")] public static FCRConfig config => new FCRConfig(resConfig);
        #endregion
    }
}

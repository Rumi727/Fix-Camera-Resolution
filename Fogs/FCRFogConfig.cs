using BepInEx.Configuration;
using LethalConfig.ConfigItems;
using LethalConfig;
using LethalConfig.ConfigItems.Options;

namespace Rumi.FixCameraResolutions.Fogs
{
    public sealed class FCRFogConfig
    {
        public bool disable
        {
            get => _disable.Value;
            set => _disable.Value = value;
        }
        readonly ConfigEntry<bool> _disable;
        public const bool dDisable = false;

        public bool alwaysUpdate
        {
            get => _alwaysUpdate.Value;
            set => _alwaysUpdate.Value = value;
        }
        readonly ConfigEntry<bool> _alwaysUpdate;
        public const bool dAlwaysUpdate = false;

        internal static FCRFogConfig? Create(ConfigFile config)
        {
            Debug.Log("Fog Config Loading...");

            try
            {
                var result = new FCRFogConfig(config);
                Debug.Log("Fog Config Loaded!");

                return result;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogWarning($"Failed to load config file\nSettings will be loaded with defaults!");
            }

            return null;
        }

        FCRFogConfig(ConfigFile config)
        {
            _disable = config.Bind("Fogs", "Disable", dDisable, "Disables fog rendering");
            _disable.SettingChanged += (sender, e) => FCRPlugin.Repatch();

            _alwaysUpdate = config.Bind("Fogs", "Always Update", dDisable, "Prevents fog from being removed from mod planets by updating every frame instead of just at startup\nEnable only when necessary as it may affect performance");
            _alwaysUpdate.SettingChanged += (sender, e) => FCRPlugin.Repatch();

            try
            {
                LethalConfigPatch();
            }
            catch (System.IO.FileNotFoundException e)
            {
                Debug.LogError(e);
                Debug.LogWarning("Lethal Config Add Fail! (This is not a bug and occurs when LethalConfig is not present)");
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("Lethal Config Add Fail!");
            }
        }

        void LethalConfigPatch()
        {
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(_disable, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(_alwaysUpdate, new BoolCheckBoxOptions()
            {
                RequiresRestart = false,
                CanModifyCallback = CanModifyAlwaysUpdate
            }));
        }

        static CanModifyResult CanModifyAlwaysUpdate() => (FCRPlugin.fogConfig?.disable ?? dDisable, "Fog patch is disabled and cannot be modified");
    }
}

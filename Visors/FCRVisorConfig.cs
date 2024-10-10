using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;

namespace Rumi.FixCameraResolutions.Visors
{
    public sealed class FCRVisorConfig
    {
        public bool disable
        {
            get => _disable.Value;
            set => _disable.Value = value;
        }
        readonly ConfigEntry<bool> _disable;
        public const bool dDisable = false;

        internal static FCRVisorConfig? Create(ConfigFile config)
        {
            Debug.Log("Visor Config Loading...");

            try
            {
                var result = new FCRVisorConfig(config);
                Debug.Log("Visor Config Loaded!");

                return result;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogWarning($"Failed to load config file\nSettings will be loaded with defaults!");
            }

            return null;
        }

        FCRVisorConfig(ConfigFile config)
        {
            _disable = config.Bind("Visors", "Disable", dDisable, "Disables visor rendering");
            _disable.SettingChanged += (sender, e) => FCRPlugin.Repatch();

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

        void LethalConfigPatch() => LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(_disable, false));
    }
}

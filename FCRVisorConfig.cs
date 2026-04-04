using System;
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

        internal static FCRVisorConfig Create(ConfigFile config)
        {
            Debug.Log("Visor Config Loading...");
            try
            {
                var result = new FCRVisorConfig(config);
                Debug.Log("Visor Config Loaded!");
                return result;
            }
            catch (Exception data)
            {
                Debug.LogError(data);
                Debug.LogWarning("Failed to load config file\nSettings will be loaded with defaults!");
            }
            return null;
        }

        private FCRVisorConfig(ConfigFile config)
        {
            _disable = config.Bind<bool>("Visors", "Disable", false, new ConfigDescription("Disables visor rendering"));
            _disable.SettingChanged += (sender, e) => FCRPlugin.Repatch();
        }

        private readonly ConfigEntry<bool> _disable;

        public const bool dDisable = false;

        public static class LethalConfig
        {
            public static void Patch(FCRVisorConfig config)
            {
                LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(config._disable, false));
            }
        }
    }
}
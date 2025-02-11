using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;
using System.Runtime.CompilerServices;

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
        }

        public static class LethalConfig
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void Patch(FCRVisorConfig config) => LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(config._disable, false));
        }
    }
}

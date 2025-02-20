using BepInEx.Configuration;
using LethalConfig.ConfigItems;
using LethalConfig;
using System.Runtime.CompilerServices;
using LethalConfig.ConfigItems.Options;

namespace Rumi.FixCameraResolutions.HUD
{
    public class FCRHUDConfig
    {
        public bool fixedAspectRatio
        {
            get => _fixedAspectRatio.Value;
            set => _fixedAspectRatio.Value = value;
        }
        readonly ConfigEntry<bool> _fixedAspectRatio;
        public const bool dFixedAspectRatio = true;

        public bool checkResolutionEveryFrame
        {
            get => _checkResolutionEveryFrame.Value;
            set => _checkResolutionEveryFrame.Value = value;
        }
        readonly ConfigEntry<bool> _checkResolutionEveryFrame;
        public const bool dCheckResolutionEveryFrame = false;

        internal static FCRHUDConfig? Create(ConfigFile config)
        {
            Debug.Log("HUD Config Loading...");

            try
            {
                var result = new FCRHUDConfig(config);
                Debug.Log("HUD Config Loaded!");

                return result;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogWarning($"Failed to load config file\nSettings will be loaded with defaults!");
            }

            return null;
        }

        FCRHUDConfig(ConfigFile config)
        {
            _fixedAspectRatio = config.Bind("HUD", "Fixed Aspect Ratio", dFixedAspectRatio, "Turn off to adjust the UI scaling dynamically instead of keeping the default fixed ratio.");
            _fixedAspectRatio.SettingChanged += (sender, e) => FCRPlugin.Repatch();

            _checkResolutionEveryFrame = config.Bind("HUD", "Check Resolution Every Frame", dCheckResolutionEveryFrame, "Checks if the resolution has changed every frame and updates accordingly. May affect performance.");
            _checkResolutionEveryFrame.SettingChanged += (sender, e) => FCRPlugin.Repatch();
        }

        public static class LethalConfig
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void Patch(FCRHUDConfig config)
            {
                LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(config._fixedAspectRatio, false));

                LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(config._checkResolutionEveryFrame, new BoolCheckBoxOptions()
                {
                    RequiresRestart = false,
                    CanModifyCallback = CanModifyFrame
                }));

                LethalConfigManager.AddConfigItem(new GenericButtonConfigItem("HUD", "Refresh resolution", "If the resolution has been released for some reason, you can refresh it using this button.", "Refresh", () => FCRHUDPatches.UpdateHUDManager(HUDManager.Instance)));
            }

            static CanModifyResult CanModifyFrame() => (!(FCRPlugin.hudConfig?.fixedAspectRatio ?? dFixedAspectRatio), "Fixed Aspect Ratio is enabled and cannot be modified");
        }
    }
}

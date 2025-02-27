﻿using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using System.Runtime.CompilerServices;

namespace Rumi.FixCameraResolutions.Resolutions
{
    public sealed class FCRResConfig
    {
        public bool enable
        {
            get => _enable.Value;
            set => _enable.Value = value;
        }
        readonly ConfigEntry<bool> _enable;
        public const bool dEnable = true;

        public bool autoSize
        {
            get => _autoSize.Value;
            set => _autoSize.Value = value;
        }
        readonly ConfigEntry<bool> _autoSize;
        public const bool dAutoSize = true;

        public int width
        {
            get => _width.Value;
            set => _width.Value = value;
        }
        readonly ConfigEntry<int> _width;
        public const int dWidth = 1920;

        public int height
        {
            get => _height.Value;
            set => _height.Value = value;
        }
        readonly ConfigEntry<int> _height;
        public const int dHeight = 1080;

        public bool checkResolutionEveryFrame
        {
            get => _checkResolutionEveryFrame.Value;
            set => _checkResolutionEveryFrame.Value = value;
        }
        readonly ConfigEntry<bool> _checkResolutionEveryFrame;
        public const bool dCheckResolutionEveryFrame = false;

        internal static FCRResConfig? Create(ConfigFile config)
        {
            Debug.Log("Resolution Config Loading...");

            try
            {
                var result = new FCRResConfig(config);
                Debug.Log("Resolution Config Loaded!");

                return result;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogWarning($"Failed to load config file\nSettings will be loaded with defaults!");
            }

            return null;
        }

        FCRResConfig(ConfigFile config)
        {
            _enable = config.Bind("Resolutions", "Enable", dEnable, "When enabled, the camera resolution will be modified.");
            _enable.SettingChanged += (sender, e) => FCRPlugin.Repatch();

            _autoSize = config.Bind("Resolutions", "Auto Size", dAutoSize, "When enabled, sets the camera size to the size of the current game window.");
            _autoSize.SettingChanged += (sender, e) => FCRResPatches.UpdateAll();

            _checkResolutionEveryFrame = config.Bind("Resolutions", "Check Resolution Every Frame", dCheckResolutionEveryFrame, "Checks if the resolution has changed every frame and updates accordingly. May affect performance.");
            _checkResolutionEveryFrame.SettingChanged += (sender, e) => FCRPlugin.Repatch();

            _width = config.Bind("Resolutions", "Width", dWidth, new ConfigDescription(string.Empty, new AcceptableValueRange<int>(10, 3840)));
            _width.SettingChanged += (sender, e) => FCRResPatches.UpdateAll();

            _height = config.Bind("Resolutions", "Height", dHeight, new ConfigDescription(string.Empty, new AcceptableValueRange<int>(10, 2160)));
            _height.SettingChanged += (sender, e) => FCRResPatches.UpdateAll();
        }

        public static class LethalConfig
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void Patch(FCRResConfig config)
            {
                LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(config._enable, false));

                LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(config._autoSize, new BoolCheckBoxOptions()
                {
                    RequiresRestart = false,
                    CanModifyCallback = CanModifyAutoSize
                }));

                LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(config._checkResolutionEveryFrame, new BoolCheckBoxOptions()
                {
                    RequiresRestart = false,
                    CanModifyCallback = CanModifyFrame
                }));

                LethalConfigManager.AddConfigItem(new IntSliderConfigItem(config._width, new IntSliderOptions()
                {
                    Min = 10,
                    Max = 3840,
                    RequiresRestart = false,
                    CanModifyCallback = CanModifySize
                }));

                LethalConfigManager.AddConfigItem(new IntSliderConfigItem(config._height, new IntSliderOptions()
                {
                    Min = 10,
                    Max = 2160,
                    RequiresRestart = false,
                    CanModifyCallback = CanModifySize
                }));

                LethalConfigManager.AddConfigItem(new GenericButtonConfigItem("Resolutions", "Refresh resolution", "If the resolution has been released for some reason, you can refresh it using this button.", "Refresh", () => FCRResPatches.UpdateAll()));
            }

            static CanModifyResult CanModifyAutoSize() => (FCRPlugin.resConfig?.enable ?? dEnable, "Resolution patch is disabled and cannot be modified");

            static CanModifyResult CanModifySize()
            {
                var result = CanModifyAutoSize();
                return !result ? result : (!(FCRPlugin.resConfig?.autoSize ?? dAutoSize), "Since auto size is enabled, the size is automatically set to the current game window size.");
            }

            static CanModifyResult CanModifyFrame()
            {
                var result = CanModifyAutoSize();
                return !result ? result : (FCRPlugin.resConfig?.autoSize ?? dAutoSize, "Auto size is disabled and cannot be modified");
            }
        }
    }
}

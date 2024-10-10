﻿using BepInEx.Configuration;
using LethalConfig.ConfigItems.Options;
using LethalConfig.ConfigItems;
using LethalConfig;
using System.IO;
using System;

namespace Rumi.FixCameraResolutions
{
    public class FCRResConfig
    {
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

        internal FCRResConfig(ConfigFile config)
        {
            _autoSize = config.Bind("Resolutions", "Auto Size", dAutoSize, "When activated, sets the camera size to the size of the current game window.");
            _autoSize.SettingChanged += (sender, e) => FCRResPatches.AllTerminalPatch();

            _width = config.Bind("Resolutions", "Width", dWidth);
            _width.SettingChanged += (sender, e) => FCRResPatches.AllTerminalPatch();

            _height = config.Bind("Resolutions", "Height", dHeight);
            _height.SettingChanged += (sender, e) => FCRResPatches.AllTerminalPatch();

            try
            {
                LethalConfigPatch();
            }
            catch (FileNotFoundException e)
            {
                Debug.LogError(e);
                Debug.LogWarning("Lethal Config Patch Fail! (This is not a bug and occurs when LethalConfig is not present)");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("Lethal Config Patch Fail!");
            }

            #region ~ 1.0.2 호환성
            {
                ConfigDefinition configDefinition = new ConfigDefinition("General", "Auto Size");
                if (config.TryGetEntry<bool>(configDefinition, out var entry))
                {
                    _autoSize.Value = entry.Value;
                    config.Remove(configDefinition);
                }
            }
            {
                ConfigDefinition configDefinition = new ConfigDefinition("General", "Width");
                if (config.TryGetEntry<int>(configDefinition, out var entry))
                { 
                    _width.Value = entry.Value;
                    config.Remove(configDefinition);
                }
            }
            {
                ConfigDefinition configDefinition = new ConfigDefinition("General", "Height");
                if (config.TryGetEntry<int>(configDefinition, out var entry))
                {
                    _height.Value = entry.Value;
                    config.Remove(configDefinition);
                }
            }
            #endregion
        }

        void LethalConfigPatch()
        {
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(_autoSize, false));

            LethalConfigManager.AddConfigItem(new IntSliderConfigItem(_width, new IntSliderOptions()
            {
                Min = 1,
                Max = 3840,
                RequiresRestart = false,
                CanModifyCallback = CanModifyAutoSize
            }));

            LethalConfigManager.AddConfigItem(new IntSliderConfigItem(_height, new IntSliderOptions()
            {
                Min = 1,
                Max = 2160,
                RequiresRestart = false,
                CanModifyCallback = CanModifyAutoSize
            }));

            LethalConfigManager.AddConfigItem(new GenericButtonConfigItem("Resolutions", "Refresh resolution", "If the resolution has been released for some reason, you can refresh it using this button.", "Refresh", () => FCRResPatches.AllTerminalPatch()));
        }

        static CanModifyResult CanModifyAutoSize() => (!FCRPlugin.resConfig?.autoSize ?? dAutoSize, "Since auto size is enabled, the size is automatically set to the current game window size.");
    }
}

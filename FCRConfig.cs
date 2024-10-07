using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using System;
using System.IO;

namespace Rumi.FixCameraResolutions
{
    public class FCRConfig
    {
        public bool autoSize
        {
            get => _autoSize?.Value ?? dAutoSize;
            set
            {
                if (_autoSize != null)
                    _autoSize.Value = value;
            }
        }
        readonly ConfigEntry<bool>? _autoSize;
        public const bool dAutoSize = true;

        public int width
        {
            get => _width?.Value ?? dWidth;
            set
            {
                if (_width != null)
                    _width.Value = value;
            }
        }
        readonly ConfigEntry<int>? _width;
        public const int dWidth = 1920;

        public int height
        {
            get => _height?.Value ?? dHeight;
            set
            {
                if (_height != null)
                    _height.Value = value;
            }
        }
        readonly ConfigEntry<int>? _height;
        public const int dHeight = 1080;

        public FCRConfig(ConfigFile config)
        {
            _autoSize = config.Bind("General", "Auto Size", dAutoSize, "When activated, sets the camera size to the size of the current game window.");
            _autoSize.SettingChanged += (sender, e) => FCRPatches.AllCameraPatch();

            _width = config.Bind("General", "Width", dWidth);
            _width.SettingChanged += (sender, e) => FCRPatches.AllCameraPatch();

            _height = config.Bind("General", "Height", dHeight);
            _height.SettingChanged += (sender, e) => FCRPatches.AllCameraPatch();

            try
            {
                LethalConfigPatch();
            }
            catch (FileNotFoundException e)
            {
                FCRPlugin.logger?.LogError(e);
                FCRPlugin.logger?.LogWarning("Lethal Config Patch Fail! (This is not a bug and occurs when LethalConfig is not present)");
            }
            catch (Exception e)
            {
                FCRPlugin.logger?.LogError(e);
                FCRPlugin.logger?.LogError("Lethal Config Patch Fail!");
            }
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
        }

        static CanModifyResult CanModifyAutoSize() => (FCRPlugin.config?.autoSize ?? dAutoSize, "Since auto size is enabled, the size is automatically set to the current game window size.");
    }
}

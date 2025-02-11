using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;
using System.Runtime.CompilerServices;

namespace Rumi.FixCameraResolutions.Fogs
{
    public sealed class FCRHDRPConfig
    {
        public AntialiasingMode antialiasingMode
        {
            get => _antialiasingMode.Value;
            set => _antialiasingMode.Value = value;
        }
        readonly ConfigEntry<AntialiasingMode> _antialiasingMode;
        public const AntialiasingMode dAntialiasingMode = AntialiasingMode.None;

        public HDRPMode bloomMode
        {
            get => _bloomMode.Value;
            set => _bloomMode.Value = value;
        }
        readonly ConfigEntry<HDRPMode> _bloomMode;
        public const HDRPMode dBloomMode = HDRPMode.Vanilla;

        public FogMode fogMode
        {
            get => _fogMode.Value;
            set => _fogMode.Value = value;
        }
        readonly ConfigEntry<FogMode> _fogMode;
        public const FogMode dFogMode = FogMode.Vanilla;

        public HDRPMode shadowMode
        {
            get => _shadowMode.Value;
            set => _shadowMode.Value = value;
        }
        readonly ConfigEntry<HDRPMode> _shadowMode;
        public const HDRPMode dShadowMode = HDRPMode.Vanilla;

        public HDRPMode postProcessingMode
        {
            get => _postProcessingMode.Value;
            set => _postProcessingMode.Value = value;
        }
        readonly ConfigEntry<HDRPMode> _postProcessingMode;
        public const HDRPMode dPostProcessingMode = HDRPMode.Vanilla;

        public HDRPMode vignetteMode
        {
            get => _vignetteMode.Value;
            set => _vignetteMode.Value = value;
        }
        readonly ConfigEntry<HDRPMode> _vignetteMode;
        public const HDRPMode dVignetteMode = HDRPMode.Vanilla;

        internal static FCRHDRPConfig? Create(ConfigFile config)
        {
            Debug.Log("HDRP Config Loading...");

            try
            {
                var result = new FCRHDRPConfig(config);
                Debug.Log("HDRP Config Loaded!");

                return result;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.LogWarning($"Failed to load config file\nSettings will be loaded with defaults!");
            }

            return null;
        }

        FCRHDRPConfig(ConfigFile config)
        {
            _antialiasingMode = config.Bind("HDRP", "Antialiasing", dAntialiasingMode);
            _bloomMode = config.Bind("HDRP", "Bloom", dBloomMode);
            _fogMode = config.Bind("HDRP", "Fog Rendering Method", dFogMode,
@"Sets how fog is rendered.

-------------------------


Vanilla
-------------------------
Use default fog settings


Hide
-------------------------
It doesn't completely remove the fog, but it adjusts it so that it doesn't obstruct your vision.


Disable
-------------------------
Completely disables all fog rendering.


ForceDisable
-------------------------
Attempts to completely disable all fog rendering on every frame.

Can be used when fog removal is not working on a mod planet

Do not use this setting unless you have a problem


-------------------------

");
            _shadowMode = config.Bind("HDRP", "Shadow", dShadowMode);
            _postProcessingMode = config.Bind("HDRP", "Post Processing", dPostProcessingMode);
            _vignetteMode = config.Bind("HDRP", "Vignette", dVignetteMode);

            _antialiasingMode.SettingChanged += (sender, e) => FCRPlugin.Repatch();
            _bloomMode.SettingChanged += (sender, e) => FCRPlugin.Repatch();
            _fogMode.SettingChanged += (sender, e) => FCRPlugin.Repatch();
            _shadowMode.SettingChanged += (sender, e) => FCRPlugin.Repatch();
            _postProcessingMode.SettingChanged += (sender, e) => FCRPlugin.Repatch();
            _vignetteMode.SettingChanged += (sender, e) => FCRPlugin.Repatch();
        }

        public static class LethalConfig
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void Patch(FCRHDRPConfig config)
            {
                LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<AntialiasingMode>(config._antialiasingMode, false));
                LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<HDRPMode>(config._bloomMode, false));
                LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<FogMode>(config._fogMode, false));
                LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<HDRPMode>(config._shadowMode, false));
                LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<HDRPMode>(config._postProcessingMode, false));
                LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<HDRPMode>(config._vignetteMode, false));
            }
        }
    }
}

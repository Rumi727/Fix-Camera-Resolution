using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;

namespace Rumi.FixCameraResolutions.Fogs
{
    public sealed class FCRFogConfig
    {
        public FogMode fogMode
        {
            get => _fogMode.Value;
            set => _fogMode.Value = value;
        }
        readonly ConfigEntry<FogMode> _fogMode;
        public const FogMode dFogMode = FogMode.Vanilla;

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
            _fogMode = config.Bind("Fogs", "Fog Rendering Method", dFogMode,
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
            _fogMode.SettingChanged += (sender, e) => FCRPlugin.Repatch();

            #region ~ 1.0.2
            {
                config.Bind("Fogs", "Disable", false);
                config.Remove(new ConfigDefinition("Fogs", "Disable"));
            }

            {
                config.Bind("Fogs", "Always Update", false);
                config.Remove(new ConfigDefinition("Fogs", "Always Update"));
            }
            #endregion

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

        void LethalConfigPatch() => LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<FogMode>(_fogMode, false));
    }
}

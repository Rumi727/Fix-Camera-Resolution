using BepInEx.Configuration;
using Rumi.FixCameraResolutions.Resolutions;
using System;

#pragma warning disable IDE0130 // 네임스페이스가 폴더 구조와 일치하지 않습니다.
namespace Rumi.FixCameraResolutions
#pragma warning restore IDE0130 // 네임스페이스가 폴더 구조와 일치하지 않습니다.
{
    [Obsolete("Deprecated class name! Please use FCRResConfig")]
    public class FCRConfig
    {
        readonly FCRResConfig? config;

        [Obsolete("Deprecated class name! Please use FCRResConfig.autoSize")]
        public bool autoSize
        {
            get => config?.autoSize ?? dAutoSize;
            set
            {
                if (config != null)
                    config.autoSize = value;
            }
        }
        [Obsolete("Deprecated class name! Please use FCRResConfig.dAutoSize")] public const bool dAutoSize = true;

        [Obsolete("Deprecated class name! Please use FCRResConfig.width")]
        public int width
        {
            get => config?.width ?? dWidth;
            set
            {
                if (config != null)
                    config.width = value;
            }
        }
        [Obsolete("Deprecated class name! Please use FCRResConfig.dWidth")] public const int dWidth = 1920;

        [Obsolete("Deprecated class name! Please use FCRResConfig.height")]
        public int height
        {
            get => config?.height ?? dHeight;
            set
            {
                if (config != null)
                    config.height = value;
            }
        }
        [Obsolete("Deprecated class name! Please use FCRResConfig.dHeight")] public const int dHeight = 1080;

        [Obsolete("Deprecated class name! Please use FCRResConfig", true)] public FCRConfig(ConfigFile config) => throw new NotImplementedException();
        internal FCRConfig(FCRResConfig? config) => this.config = config;
    }
}

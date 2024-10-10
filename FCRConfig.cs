using BepInEx.Configuration;
using System;

namespace Rumi.FixCameraResolutions
{
    [Obsolete("Deprecated class name! Please use FCRResConfig", true)]
    public class FCRConfig
    {
        [Obsolete("Deprecated class name! Please use FCRResConfig.autoSize", true)]
        public bool autoSize
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        [Obsolete("Deprecated class name! Please use FCRResConfig.dAutoSize", true)] public const bool dAutoSize = true;

        [Obsolete("Deprecated class name! Please use FCRResConfig.width", true)]
        public int width
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        [Obsolete("Deprecated class name! Please use FCRResConfig.dWidth", true)] public const int dWidth = 1920;

        [Obsolete("Deprecated class name! Please use FCRResConfig.height", true)]
        public int height
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        [Obsolete("Deprecated class name! Please use FCRResConfig.dHeight", true)] public const int dHeight = 1080;

        [Obsolete("Deprecated class name! Please use FCRResConfig", true)] public FCRConfig(ConfigFile config) => throw new NotImplementedException();
    }
}

namespace Rumi.FixCameraResolutions
{
    static class Debug
    {
        public static void Log(object data) => FCRPlugin.logger?.LogInfo(data);
        public static void LogWarning(object data) => FCRPlugin.logger?.LogWarning(data);
        public static void LogError(object data) => FCRPlugin.logger?.LogError(data);
    }
}

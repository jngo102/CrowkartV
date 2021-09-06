using UnityEngine;

namespace CrowkartVRMod.Utilities
{
    internal static class LogUtils
    {
        private static readonly string _tag = "[CrowkartVR] ";

        public static void Log(object message)
        {
            Debug.Log(_tag + message);
        }

        public static void LogWarning(object message)
        {
            Debug.LogWarning(_tag + message);
        }

        public static void LogError(object message)
        {
            Debug.LogError(_tag + message);
        }
    }
}

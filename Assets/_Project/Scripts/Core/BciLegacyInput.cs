using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// 统一使用旧版 Input Manager（Project Settings → Active Input Handling = Input Manager）。
    /// 使用 UnityEngine.Input 全名，避免与 ADHDTraining.Core.Input 命名空间冲突。
    /// </summary>
    internal static class BciLegacyInput
    {
        public static float MouseScrollWheel => UnityEngine.Input.GetAxis("Mouse ScrollWheel");

        public static bool GetKeyDown(KeyCode key) => UnityEngine.Input.GetKeyDown(key);
        public static bool GetKey(KeyCode key) => UnityEngine.Input.GetKey(key);
    }
}

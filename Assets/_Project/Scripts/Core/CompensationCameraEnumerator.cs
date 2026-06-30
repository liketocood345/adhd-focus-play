using ADHDTraining.Core.MediaPipe;

namespace ADHDTraining.Core
{
    /// <summary>
    /// 通过 MediaPipe bridge 枚举 DirectShow 摄像头。
    /// </summary>
    public static class CompensationCameraEnumerator
    {
        public static bool TryList(string exePath, out string[] cameras, out string error)
        {
            cameras = MediaPipe.MediaPipeBridgeLauncher.ListCameras(
                string.IsNullOrWhiteSpace(exePath) ? null : exePath);
            error = cameras.Length == 0 ? "MediaPipe 未检测到摄像头" : null;
            return cameras.Length > 0;
        }
    }
}

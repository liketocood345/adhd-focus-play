using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ADHDTraining.Core
{
    public enum CompensationCaptureKind
    {
        Camera = 0,
        VideoFile = 1
    }

    /// <summary>
    /// 代偿 OpenSeeFace 视频输入源（摄像头名称/索引或视频文件路径），持久化到 PlayerPrefs。
    /// </summary>
    public static class CompensationVideoSourceStore
    {
        private const string KindKey = "adhd_comp_capture_kind";
        private const string CameraKey = "adhd_comp_camera_index";
        private const string CameraNameKey = "adhd_comp_camera_name";
        private const string VideoKey = "adhd_comp_video_path";

        public static CompensationCaptureKind Kind { get; private set; } = CompensationCaptureKind.Camera;
        public static int CameraIndex { get; private set; }
        public static string CameraName { get; private set; } = "";
        public static string VideoPath { get; private set; } = "";

        public static void Load()
        {
            Kind = (CompensationCaptureKind)PlayerPrefs.GetInt(KindKey, (int)CompensationCaptureKind.Camera);
            CameraIndex = PlayerPrefs.GetInt(CameraKey, 0);
            CameraName = PlayerPrefs.GetString(CameraNameKey, "");
            VideoPath = PlayerPrefs.GetString(VideoKey, "");
        }

        public static void SetCamera(int index, string deviceName = null)
        {
            Kind = CompensationCaptureKind.Camera;
            UpdateCameraSelection(index, deviceName);
            PlayerPrefs.SetInt(KindKey, (int)Kind);
            PlayerPrefs.Save();
        }

        /// <summary>仅更新摄像头索引/名称，不改变摄像头/视频模式。</summary>
        public static void UpdateCameraSelection(int index, string deviceName = null)
        {
            CameraIndex = Mathf.Max(0, index);
            if (!string.IsNullOrWhiteSpace(deviceName))
                CameraName = deviceName.Trim();
            PlayerPrefs.SetInt(CameraKey, CameraIndex);
            PlayerPrefs.SetString(CameraNameKey, CameraName ?? "");
            PlayerPrefs.Save();
        }

        public static void SetVideoFile(string path)
        {
            Kind = CompensationCaptureKind.VideoFile;
            VideoPath = path ?? "";
            Save();
        }

        public static void SelectKind(CompensationCaptureKind kind)
        {
            Kind = kind;
            Save();
        }

        public static void Save()
        {
            PlayerPrefs.SetInt(KindKey, (int)Kind);
            PlayerPrefs.SetInt(CameraKey, CameraIndex);
            PlayerPrefs.SetString(CameraNameKey, CameraName ?? "");
            PlayerPrefs.SetString(VideoKey, VideoPath ?? "");
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 在枚举列表中按已记忆的设备名匹配索引；名称失效时回退到记忆的索引。
        /// </summary>
        public static int ResolveCameraIndex(IReadOnlyList<string> cameras)
        {
            Load();
            if (cameras == null || cameras.Count == 0)
                return 0;

            if (!string.IsNullOrWhiteSpace(CameraName))
            {
                for (var i = 0; i < cameras.Count; i++)
                {
                    if (IsPlaceholderEntry(cameras[i])) continue;
                    if (string.Equals(cameras[i], CameraName, System.StringComparison.OrdinalIgnoreCase))
                        return i;
                }

                for (var i = 0; i < cameras.Count; i++)
                {
                    if (IsPlaceholderEntry(cameras[i])) continue;
                    if (cameras[i].IndexOf(CameraName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return i;
                }
            }

            return Mathf.Clamp(CameraIndex, 0, cameras.Count - 1);
        }

        public static bool IsVideoPathValid()
        {
            Load();
            return Kind == CompensationCaptureKind.VideoFile
                   && !string.IsNullOrEmpty(VideoPath)
                   && File.Exists(VideoPath);
        }

        public static string Summary()
        {
            Load();
            if (Kind == CompensationCaptureKind.VideoFile)
            {
                if (string.IsNullOrEmpty(VideoPath))
                    return "视频：未选择（已记忆：视频文件模式）";
                var file = Path.GetFileName(VideoPath);
                return File.Exists(VideoPath)
                    ? $"视频：{file}（已记忆）"
                    : $"视频：{file}（文件不存在）";
            }

            if (!string.IsNullOrWhiteSpace(CameraName))
                return $"摄像头：{CameraName}（已记忆）";
            return $"摄像头 #{CameraIndex}（已记忆）";
        }

        private static bool IsPlaceholderEntry(string name) =>
            !string.IsNullOrEmpty(name) && name.StartsWith("（未检测到");
    }
}

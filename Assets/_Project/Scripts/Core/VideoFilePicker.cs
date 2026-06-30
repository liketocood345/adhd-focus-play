using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// 选择本地视频文件（Editor 或 Windows 独立构建）。
    /// </summary>
    public static class VideoFilePicker
    {
        public static bool TryPickVideo(out string path)
        {
            path = null;
#if UNITY_EDITOR
            CompensationVideoSourceStore.Load();
            path = UnityEditor.EditorUtility.OpenFilePanel(
                "选择代偿视频输入",
                string.IsNullOrEmpty(CompensationVideoSourceStore.VideoPath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
                    : Path.GetDirectoryName(CompensationVideoSourceStore.VideoPath),
                "mp4,avi,mov,wmv,mkv,webm");
            return !string.IsNullOrEmpty(path);
#elif UNITY_STANDALONE_WIN
            return WindowsOpenFile.TryPick("选择代偿视频输入", "Video files\0*.mp4;*.avi;*.mov;*.wmv;*.mkv;*.webm\0All files\0*.*\0", out path);
#else
            Debug.LogWarning("[代偿] 当前平台无原生文件对话框，请手动填写视频路径。");
            return false;
#endif
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private static class WindowsOpenFile
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            private struct OpenFileName
            {
                public int lStructSize;
                public IntPtr hwndOwner;
                public IntPtr hInstance;
                public string lpstrFilter;
                public string lpstrCustomFilter;
                public int nMaxCustFilter;
                public int nFilterIndex;
                public string lpstrFile;
                public int nMaxFile;
                public string lpstrFileTitle;
                public int nMaxFileTitle;
                public string lpstrInitialDir;
                public string lpstrTitle;
                public int Flags;
                public short nFileOffset;
                public short nFileExtension;
                public string lpstrDefExt;
                public IntPtr lCustData;
                public IntPtr lpfnHook;
                public string lpTemplateName;
                public IntPtr pvReserved;
                public int dwReserved;
                public int flagsEx;
            }

            [DllImport("comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool GetOpenFileName(ref OpenFileName ofn);

            public static bool TryPick(string title, string filter, out string path)
            {
                path = null;
                var buffer = new StringBuilder(260);
                var ofn = new OpenFileName
                {
                    lStructSize = Marshal.SizeOf<OpenFileName>(),
                    lpstrFilter = filter,
                    lpstrFile = buffer.ToString(),
                    nMaxFile = buffer.Capacity,
                    lpstrTitle = title,
                    Flags = 0x00080000 | 0x00001000 | 0x00000800
                };

                ofn.lpstrFile = new string('\0', 256);
                if (!GetOpenFileName(ref ofn)) return false;
                path = ofn.lpstrFile.TrimEnd('\0');
                return !string.IsNullOrEmpty(path);
            }
        }
#endif
    }
}

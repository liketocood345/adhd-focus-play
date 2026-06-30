using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace ADHDTraining.Core.MediaPipe
{
  public class MediaPipeBridgeLauncher
  {
    public const string DefaultPython =
      @"f:\EasyVtuber\EasyVtuber_v0.8.1\EasyVtuber_v0.8.1\envs\python_embedded\python.exe";

    private Process _process;
    private string _pythonExe = DefaultPython;
    private readonly string _scriptPath;
    private string _lastError;

    public string LastError => _lastError;
    public bool IsRunning => _process != null && !_process.HasExited;

    public MediaPipeBridgeLauncher()
    {
      _scriptPath = ResolveScriptPath();
    }

    public void SetPythonPath(string path)
    {
      if (!string.IsNullOrWhiteSpace(path))
        _pythonExe = path;
    }

    public static string ResolveScriptPath()
    {
      var fromData = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Tools", "mediapipe_bridge", "tracker.py"));
      if (File.Exists(fromData)) return fromData;
      return Path.Combine(Directory.GetCurrentDirectory(), "Tools", "mediapipe_bridge", "tracker.py");
    }

    public bool Start(MediaPipeLaunchArgs args)
    {
      Stop();
      if (!File.Exists(_scriptPath))
      {
        _lastError = $"未找到 tracker.py: {_scriptPath}";
        UnityEngine.Debug.LogError($"[MediaPipe] {_lastError}");
        return false;
      }

      var python = ResolvePython(_pythonExe);
      if (python == null)
      {
        _lastError = "未找到 Python（请安装 mediapipe 或配置 EasyVtuber 嵌入式 Python）";
        UnityEngine.Debug.LogError($"[MediaPipe] {_lastError}");
        return false;
      }

      var sb = new StringBuilder();
      sb.Append('"').Append(_scriptPath).Append('"');
      sb.Append($" --host 127.0.0.1 --port {args.Port}");
      sb.Append($" --camera {args.CameraIndex}");
      sb.Append($" --width {args.Width} --height {args.Height} --fps {args.Fps}");
      sb.Append($" --preview {(args.Preview ? 1 : 0)}");
      sb.Append($" --pose-complexity {args.PoseComplexity}");
      if (!string.IsNullOrEmpty(args.VideoPath))
        sb.Append($" --video \"{args.VideoPath}\"");

      var psi = new ProcessStartInfo
      {
        FileName = python,
        Arguments = sb.ToString(),
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        WorkingDirectory = Path.GetDirectoryName(_scriptPath) ?? ""
      };

      try
      {
        _process = Process.Start(psi);
        _lastError = null;
        UnityEngine.Debug.Log($"[MediaPipe] 启动: {python} {psi.Arguments}");
        return _process != null;
      }
      catch (Exception ex)
      {
        _lastError = ex.Message;
        UnityEngine.Debug.LogError($"[MediaPipe] 启动失败: {ex.Message}");
        return false;
      }
    }

    public void Stop()
    {
      if (_process == null) return;
      try
      {
        if (!_process.HasExited)
          _process.Kill();
      }
      catch { /* ignore */ }
      finally
      {
        _process.Dispose();
        _process = null;
      }
    }

    public static string[] ListCameras(string pythonExe = null, bool forceRefresh = false)
    {
      if (!forceRefresh && _cachedCameras != null && Time.realtimeSinceStartup - _cacheTime < 45f)
        return _cachedCameras;

      var script = ResolveScriptPath();
      var python = ResolvePython(pythonExe ?? DefaultPython);
      if (python == null || !File.Exists(script))
        return Array.Empty<string>();

      var psi = new ProcessStartInfo
      {
        FileName = python,
        Arguments = $"\"{script}\" --list-cameras",
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
      };

      try
      {
        using var p = Process.Start(psi);
        if (p == null) return Array.Empty<string>();
        var output = p.StandardOutput.ReadToEnd();
        p.WaitForExit(12000);
        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var list = new System.Collections.Generic.List<string>();
        foreach (var line in lines)
        {
          var parts = line.Split(new[] { ": " }, 2, StringSplitOptions.None);
          list.Add(parts.Length == 2 ? parts[1] : line);
        }
        _cachedCameras = list.ToArray();
        _cacheTime = Time.realtimeSinceStartup;
        return _cachedCameras;
      }
      catch
      {
        return Array.Empty<string>();
      }
    }

    private static string[] _cachedCameras;
    private static float _cacheTime;

    private static string ResolvePython(string preferred)
    {
      if (!string.IsNullOrEmpty(preferred) && File.Exists(preferred))
        return preferred;
      foreach (var name in new[] { "python", "python3" })
      {
        try
        {
          var psi = new ProcessStartInfo(name, "--version")
          {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true
          };
          using var p = Process.Start(psi);
          if (p != null)
          {
            p.WaitForExit(3000);
            if (p.ExitCode == 0) return name;
          }
        }
        catch { /* try next */ }
      }
      return null;
    }
  }

  public struct MediaPipeLaunchArgs
  {
    public int Port;
    public int CameraIndex;
    public int Width;
    public int Height;
    public int Fps;
    public bool Preview;
    public int PoseComplexity;
    public string VideoPath;
  }
}

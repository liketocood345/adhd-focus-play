using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace ADHDTraining.Core.MediaPipe
{
  public class MediaPipeMotionClient : IDisposable
  {
    public const int DefaultPort = 9878;

    private UdpClient _client;
    private MediaPipeMotionFrame _latest;
    private int _packets;
    private float _lastPacketTime;

    public MediaPipeMotionFrame Latest => _latest;
    public int PacketsReceived => _packets;
    public bool IsReceiving => _packets > 0 && Time.realtimeSinceStartup - _lastPacketTime < 1.5f;

    public void Start(int port = DefaultPort)
    {
      Stop();
      try
      {
        _client = new UdpClient(port);
        _client.BeginReceive(OnReceived, null);
      }
      catch (Exception ex)
      {
        Debug.LogWarning($"[MediaPipe] UDP 绑定失败 :{port} — {ex.Message}");
      }
    }

    public void Stop()
    {
      if (_client == null) return;
      try { _client.Close(); } catch { /* ignore */ }
      _client = null;
    }

    public void Dispose() => Stop();

    private void OnReceived(IAsyncResult ar)
    {
      if (_client == null) return;
      try
      {
        var remote = new IPEndPoint(IPAddress.Any, 0);
        var data = _client.EndReceive(ar, ref remote);
        _client.BeginReceive(OnReceived, null);
        var json = Encoding.UTF8.GetString(data);
        if (TryParse(json, out var frame))
        {
          _latest = frame;
          _packets++;
          _lastPacketTime = Time.realtimeSinceStartup;
        }
      }
      catch (ObjectDisposedException) { }
      catch (Exception ex)
      {
        Debug.LogWarning($"[MediaPipe] 接收异常: {ex.Message}");
        try { _client?.BeginReceive(OnReceived, null); } catch { /* ignore */ }
      }
    }

    public static bool TryParse(string json, out MediaPipeMotionFrame frame)
    {
      frame = default;
      if (string.IsNullOrEmpty(json)) return false;
      try
      {
        frame.Seq = ReadInt(json, "seq");
        frame.FaceValid = ReadBool(json, "face_valid");
        frame.PoseValid = ReadBool(json, "pose_valid");
        frame.Fps = ReadFloat(json, "fps");
        frame.EyeL = ReadFloat(json, "eye_l");
        frame.EyeR = ReadFloat(json, "eye_r");
        frame.HeadPitch = ReadFloat(json, "head_pitch");
        frame.HeadYaw = ReadFloat(json, "head_yaw");
        frame.HeadRoll = ReadFloat(json, "head_roll");
        frame.GazeYaw = ReadFloat(json, "gaze_yaw");
        frame.GazePitch = ReadFloat(json, "gaze_pitch");
        frame.NeckX = ReadFloat(json, "neck_x");
        frame.NeckY = ReadFloat(json, "neck_y");
        frame.NeckZ = ReadFloat(json, "neck_z");
        frame.LsX = ReadFloat(json, "ls_x");
        frame.LsY = ReadFloat(json, "ls_y");
        frame.LsZ = ReadFloat(json, "ls_z");
        frame.RsX = ReadFloat(json, "rs_x");
        frame.RsY = ReadFloat(json, "rs_y");
        frame.RsZ = ReadFloat(json, "rs_z");
        frame.LeftHandRaised = ReadBool(json, "left_hand_raised");
        frame.RightHandRaised = ReadBool(json, "right_hand_raised");
        frame.BodyLean = ReadInt(json, "body_lean");
        return frame.FaceValid || frame.PoseValid;
      }
      catch { return false; }
    }

    private static float ReadFloat(string json, string key)
    {
      var s = ReadToken(json, key);
      return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0f;
    }

    private static int ReadInt(string json, string key)
    {
      var s = ReadToken(json, key);
      return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0;
    }

    private static bool ReadBool(string json, string key)
    {
      var s = ReadToken(json, key);
      return s == "true" || s == "1";
    }

    private static string ReadToken(string json, string key)
    {
      var needle = $"\"{key}\":";
      var i = json.IndexOf(needle, StringComparison.Ordinal);
      if (i < 0) return "";
      i += needle.Length;
      while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
      if (i >= json.Length) return "";
      if (json[i] == '"')
      {
        var j = json.IndexOf('"', i + 1);
        return j > i ? json.Substring(i + 1, j - i - 1) : "";
      }
      var k = i;
      while (k < json.Length && ",}".IndexOf(json[k]) < 0) k++;
      return json.Substring(i, k - i).Trim();
    }
  }
}

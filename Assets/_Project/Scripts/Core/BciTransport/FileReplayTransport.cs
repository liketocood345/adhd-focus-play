using System;
using System.IO;
using UnityEngine;

namespace ADHDTraining.Core.BciTransport
{
    /// <summary>
    /// 回放 JSON 行文件，每行一帧，用于离线调试脑机输入。
    /// </summary>
    public class FileReplayTransport : IExternalBciTransport
    {
        private string[] _lines;
        private int _index;
        private BciInputSnapshot _last;
        private string _raw = "";

        public bool IsConnected => _lines != null && _lines.Length > 0;

        public void Connect(BciTransportConfig config)
        {
            _lines = null;
            _index = 0;
            var path = config.replayFile;
            if (string.IsNullOrEmpty(path)) return;
            if (!Path.IsPathRooted(path))
                path = Path.Combine(Application.streamingAssetsPath, path);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[FileReplayTransport] file not found: {path}");
                return;
            }
            _lines = File.ReadAllLines(path);
        }

        public void Disconnect()
        {
            _lines = null;
            _index = 0;
        }

        public void Tick()
        {
            if (!IsConnected) return;
            _raw = _lines[_index % _lines.Length];
            _last = HybridBciUdpTransportReflectionParse(_raw);
            _index++;
        }

        public bool TryRead(out BciInputSnapshot snapshot, out string rawDebug)
        {
            snapshot = _last;
            rawDebug = _raw;
            return IsConnected;
        }

        private static BciInputSnapshot HybridBciUdpTransportReflectionParse(string json)
        {
            var cfg = new BciTransportConfig();
            var snap = new BciInputSnapshot { RawDebug = json };
            if (string.IsNullOrEmpty(json)) return snap;
            snap.Focus = ParseFloat(json, cfg.focusField, 50f);
            snap.Blink = json.IndexOf("\"blink\":true", StringComparison.OrdinalIgnoreCase) >= 0;
            if (json.IndexOf("\"nod\"", StringComparison.OrdinalIgnoreCase) >= 0) snap.Head = HeadGesture.Nod;
            else if (json.IndexOf("\"shake\"", StringComparison.OrdinalIgnoreCase) >= 0) snap.Head = HeadGesture.Shake;
            else if (json.IndexOf("\"turnleft\"", StringComparison.OrdinalIgnoreCase) >= 0) snap.Head = HeadGesture.TurnLeft;
            else if (json.IndexOf("\"turnright\"", StringComparison.OrdinalIgnoreCase) >= 0) snap.Head = HeadGesture.TurnRight;
            return snap;
        }

        private static float ParseFloat(string json, string key, float fallback)
        {
            var token = $"\"{key}\":";
            var idx = json.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return fallback;
            var start = idx + token.Length;
            var end = json.IndexOfAny(new[] { ',', '}' }, start);
            if (end < 0) end = json.Length;
            var slice = json.Substring(start, end - start).Trim();
            return float.TryParse(slice, out var v) ? v : fallback;
        }
    }
}

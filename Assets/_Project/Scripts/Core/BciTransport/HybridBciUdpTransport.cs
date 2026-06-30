using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace ADHDTraining.Core.BciTransport
{
    /// <summary>
    /// 接收 HybridBCI / Python 桥接发来的 UDP JSON 帧。
    /// 示例: {"focus":72,"blink":false,"head":"nod"}
    /// </summary>
    public class HybridBciUdpTransport : IExternalBciTransport
    {
        private UdpClient _client;
        private BciTransportConfig _config;
        private BciInputSnapshot _last;
        private string _raw = "";

        public bool IsConnected => _client != null;

        public void Connect(BciTransportConfig config)
        {
            Disconnect();
            _config = config;
            try
            {
                _client = new UdpClient(config.port);
                _client.Client.ReceiveTimeout = 1;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[HybridBciUdpTransport] bind failed: {ex.Message}");
                Disconnect();
            }
        }

        public void Disconnect()
        {
            _client?.Close();
            _client = null;
        }

        public void Tick()
        {
            if (_client == null) return;
            try
            {
                while (_client.Available > 0)
                {
                    var endpoint = new IPEndPoint(IPAddress.Any, 0);
                    var data = _client.Receive(ref endpoint);
                    _raw = Encoding.UTF8.GetString(data);
                    _last = ParseJson(_raw, _config);
                }
            }
            catch (SocketException) { }
            catch (Exception ex)
            {
                Debug.LogWarning($"[HybridBciUdpTransport] {ex.Message}");
            }
        }

        public bool TryRead(out BciInputSnapshot snapshot, out string rawDebug)
        {
            snapshot = _last;
            rawDebug = _raw;
            return IsConnected;
        }

        private static BciInputSnapshot ParseJson(string json, BciTransportConfig cfg)
        {
            var snap = new BciInputSnapshot { RawDebug = json };
            if (string.IsNullOrEmpty(json)) return snap;

            snap.Focus = ReadFloat(json, cfg.focusField, snap.Focus);
            snap.Blink = ReadBool(json, cfg.blinkField);
            snap.Head = ReadHead(json, cfg.headField);
            return snap;
        }

        private static float ReadFloat(string json, string key, float fallback)
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

        private static bool ReadBool(string json, string key)
        {
            var token = $"\"{key}\":";
            var idx = json.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return false;
            return json.IndexOf("true", idx, StringComparison.OrdinalIgnoreCase) >= 0
                   && json.IndexOf("true", idx, StringComparison.OrdinalIgnoreCase) < idx + 20;
        }

        private static HeadGesture ReadHead(string json, string key)
        {
            var token = $"\"{key}\":\"";
            var idx = json.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return HeadGesture.None;
            var start = idx + token.Length;
            var end = json.IndexOf('"', start);
            if (end < 0) return HeadGesture.None;
            var value = json.Substring(start, end - start).ToLowerInvariant();
            return value switch
            {
                "nod" => HeadGesture.Nod,
                "shake" => HeadGesture.Shake,
                "turnleft" or "left" => HeadGesture.TurnLeft,
                "turnright" or "right" => HeadGesture.TurnRight,
                _ => HeadGesture.None
            };
        }
    }
}

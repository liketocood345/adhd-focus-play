using System;
using System.IO;
using UnityEngine;

namespace ADHDTraining.Core.BciTransport
{
    public static class BciTransportFactory
    {
        public static IExternalBciTransport Create(BciTransportConfig config)
        {
            if (config == null) return new NullBciTransport();
            return config.transportType?.ToLowerInvariant() switch
            {
                "udp" => new HybridBciUdpTransport(),
                "replay" => new FileReplayTransport(),
                "serial" => new NullBciTransport(),
                _ => new NullBciTransport()
            };
        }

        public static BciTransportConfig LoadFromResources()
        {
            var text = Resources.Load<TextAsset>("bci_transport");
            if (text != null && !string.IsNullOrEmpty(text.text))
                return JsonUtility.FromJson<BciTransportConfig>(text.text);

            var path = Path.Combine(Application.dataPath, "_Project/Config/bci_transport.json");
            if (File.Exists(path))
                return JsonUtility.FromJson<BciTransportConfig>(File.ReadAllText(path));

            return new BciTransportConfig();
        }
    }
}

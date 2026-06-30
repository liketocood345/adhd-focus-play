using ADHDTraining.Core.BciTransport;
using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// HybridBCI：经 IExternalBciTransport 接收外部数据（UDP/回放等）。
    /// </summary>
    public class HybridBciInputProvider : MonoBehaviour, IBciInputProvider
    {
        [SerializeField] private float scrollFallbackFocus = 50f;
        [SerializeField] private float scrollSensitivity = 80f;
        [SerializeField] private bool allowScrollFallback = true;

        private IExternalBciTransport _transport;
        private BciInputSnapshot _current;

        public BciInputSnapshot Current => _current;
        public bool IsConnected => _transport != null && _transport.IsConnected;
        public IExternalBciTransport Transport => _transport;

        public void ConfigureTransport(IExternalBciTransport transport, BciTransportConfig config)
        {
            _transport?.Disconnect();
            _transport = transport ?? new NullBciTransport();
            _transport.Connect(config ?? new BciTransportConfig());
        }

        private void Awake()
        {
            if (_transport == null)
            {
                var config = BciTransportFactory.LoadFromResources();
                ConfigureTransport(BciTransportFactory.Create(config), config);
            }
        }

        private void OnDestroy()
        {
            _transport?.Disconnect();
        }

        private void Update()
        {
            _transport?.Tick();

            if (_transport != null && _transport.TryRead(out var snap, out var raw) && !string.IsNullOrEmpty(raw))
            {
                _current = snap;
                _current.RawDebug = raw;
            }
            else
            {
                if (allowScrollFallback)
                    scrollFallbackFocus = FocusScrollController.ApplyScroll(scrollFallbackFocus, scrollSensitivity);

                _current = new BciInputSnapshot
                {
                    Focus = scrollFallbackFocus,
                    Blink = false,
                    Head = HeadGesture.None,
                    RawDebug = IsConnected ? "waiting" : "not_connected"
                };
            }
        }
    }
}

namespace ADHDTraining.Core.BciTransport
{
    public class NullBciTransport : IExternalBciTransport
    {
        public bool IsConnected => false;

        public void Connect(BciTransportConfig config) { }

        public void Disconnect() { }

        public bool TryRead(out BciInputSnapshot snapshot, out string rawDebug)
        {
            snapshot = default;
            rawDebug = "null transport";
            return false;
        }

        public void Tick() { }
    }
}

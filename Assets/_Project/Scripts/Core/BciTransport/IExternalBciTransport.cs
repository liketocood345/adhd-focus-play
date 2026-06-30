namespace ADHDTraining.Core.BciTransport
{
    public interface IExternalBciTransport
    {
        bool IsConnected { get; }
        void Connect(BciTransportConfig config);
        void Disconnect();
        bool TryRead(out BciInputSnapshot snapshot, out string rawDebug);
        void Tick();
    }
}

namespace ADHDTraining.Core
{
    /// <summary>
    /// 接收 HybridBCI 专注力、眨眼、头动信号。
    /// 正式接入时替换为平台 SDK / 蓝牙通信实现。
    /// </summary>
    public interface IBciInputProvider
    {
        BciInputSnapshot Current { get; }
        bool IsConnected { get; }
    }
}

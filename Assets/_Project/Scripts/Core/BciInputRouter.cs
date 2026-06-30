using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// 统一 BCI 输入路由。按开关在 HybridBCI / 代偿 / 模拟 之间切换。
    /// </summary>
    public class BciInputRouter : MonoBehaviour, IBciInputProvider
    {
        [Header("Providers")]
        [SerializeField] private CompensationBciInputProvider compensation;
        [SerializeField] private MockBciInputProvider mock;
        [SerializeField] private HybridBciInputProvider hybridBci;

        [Header("Mode")]
        [SerializeField] private bool useCompensation = true;
        [SerializeField] private KeyCode toggleCompensationKey = KeyCode.C;

        public bool UseCompensation
        {
            get => useCompensation;
            set => useCompensation = value;
        }

        public BciInputMode ActiveMode
        {
            get
            {
                if (useCompensation) return BciInputMode.Compensation;
                if (hybridBci != null && hybridBci.IsConnected) return BciInputMode.HybridBci;
                return BciInputMode.Mock;
            }
        }

        public BciInputSnapshot Current => GetActiveProvider().Current;
        public bool IsConnected => GetActiveProvider().IsConnected;

        private void Update()
        {
            if (Input.GetKeyDown(toggleCompensationKey))
                useCompensation = !useCompensation;
        }

        private IBciInputProvider GetActiveProvider()
        {
            if (useCompensation && compensation != null)
                return compensation;
            if (hybridBci != null && hybridBci.IsConnected)
                return hybridBci;
            return mock != null ? mock : (IBciInputProvider)this;
        }

        public CompensationBciInputProvider Compensation => compensation;
        public MockBciInputProvider Mock => mock;
        public HybridBciInputProvider HybridBci => hybridBci;

        public void Bind(
            CompensationBciInputProvider compensationProvider,
            MockBciInputProvider mockProvider,
            HybridBciInputProvider hybridProvider)
        {
            compensation = compensationProvider;
            mock = mockProvider;
            hybridBci = hybridProvider;
        }
    }
}

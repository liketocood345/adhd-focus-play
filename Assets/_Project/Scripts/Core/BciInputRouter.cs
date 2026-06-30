using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// 统一 BCI 输入路由：代偿 / 脑机 / 纯键鼠 三模式显式切换。
    /// </summary>
    public class BciInputRouter : MonoBehaviour, IBciInputProvider
    {
        private const string PrefsKey = "adhd_bci_input_mode";

        [Header("Providers")]
        [SerializeField] private CompensationBciInputProvider compensation;
        [SerializeField] private MockBciInputProvider mock;
        [SerializeField] private HybridBciInputProvider hybridBci;

        [Header("Mode")]
        [SerializeField] private BciInputMode activeMode = BciInputMode.Compensation;
        [SerializeField] private KeyCode toggleCompensationKey = KeyCode.C;

        public BciInputMode ActiveMode => activeMode;

        /// <summary>Bootstrap 场景兼容：代偿开/关。</summary>
        public bool UseCompensation
        {
            get => activeMode == BciInputMode.Compensation;
            set => SetInputMode(value ? BciInputMode.Compensation : BciInputMode.Mock);
        }

        public BciInputSnapshot Current => GetActiveProvider().Current;
        public bool IsConnected => GetActiveProvider().IsConnected;

        public CompensationBciInputProvider Compensation => compensation;
        public MockBciInputProvider Mock => mock;
        public HybridBciInputProvider HybridBci => hybridBci;

        private void Awake()
        {
            if (PlayerPrefs.HasKey(PrefsKey))
                activeMode = (BciInputMode)PlayerPrefs.GetInt(PrefsKey, (int)BciInputMode.Compensation);
        }

        private void Update()
        {
            if (BciLegacyInput.GetKeyDown(toggleCompensationKey))
            {
                SetInputMode(activeMode == BciInputMode.Compensation
                    ? BciInputMode.Mock
                    : BciInputMode.Compensation);
            }
        }

        private void Start()
    {
        if (activeMode == BciInputMode.Compensation)
            compensation?.StartTracking();
    }

    public void SetInputMode(BciInputMode mode)
        {
            activeMode = mode;
            PlayerPrefs.SetInt(PrefsKey, (int)mode);
            PlayerPrefs.Save();

            if (mode == BciInputMode.Compensation)
            {
                compensation?.ApplyStoredVideoSource();
                compensation?.StartTracking();
            }
            else
                compensation?.StopTracking();
        }

        /// <summary>恢复 PlayerPrefs 中的模式；追踪启动由视频源面板或首次 StartTracking 统一触发。</summary>
        public void RestoreSavedMode()
        {
            if (PlayerPrefs.HasKey(PrefsKey))
                activeMode = (BciInputMode)PlayerPrefs.GetInt(PrefsKey, (int)BciInputMode.Compensation);

            compensation?.ApplyStoredVideoSource();
            if (activeMode != BciInputMode.Compensation)
                compensation?.StopTracking();
        }

        private IBciInputProvider GetActiveProvider()
        {
            switch (activeMode)
            {
                case BciInputMode.Compensation when compensation != null:
                    return compensation;
                case BciInputMode.HybridBci when hybridBci != null:
                    return hybridBci;
                case BciInputMode.Mock when mock != null:
                    return mock;
            }

            if (compensation != null) return compensation;
            if (hybridBci != null) return hybridBci;
            return mock != null ? mock : (IBciInputProvider)this;
        }

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

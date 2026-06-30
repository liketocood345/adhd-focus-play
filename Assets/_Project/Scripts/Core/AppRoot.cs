using ADHDTraining.Core.MediaPipe;
using ADHDTraining.Core.Session;
using ADHDTraining.UI;
using UnityEngine;

namespace ADHDTraining.Core
{
    public class AppRoot : MonoBehaviour
    {
        private static AppRoot _instance;

        [Header("MediaPipe (compensation)")]
        [SerializeField] private string pythonExe = MediaPipeBridgeLauncher.DefaultPython;

        public static AppRoot Instance => _instance;
        public BciInputRouter Router { get; private set; }
        public SessionRecordService Records { get; private set; }
        public AppHudController Hud { get; private set; }
        public UI.GazeCalibrationController GazeCalibration { get; private set; }

        public static AppRoot Ensure()
        {
            if (_instance != null) return _instance;
            var go = new GameObject("AppRoot");
            return go.AddComponent<AppRoot>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            BuildServices();
        }

        private void BuildServices()
        {
            Records = gameObject.AddComponent<SessionRecordService>();

            var compensation = gameObject.AddComponent<CompensationBciInputProvider>();
            var mock = gameObject.AddComponent<MockBciInputProvider>();
            var hybrid = gameObject.AddComponent<HybridBciInputProvider>();

            compensation.ConfigurePaths(pythonExe, null, 0);

            GazeCalibration = gameObject.AddComponent<UI.GazeCalibrationController>();
            compensation.SetGazeMapper(GazeCalibration.Mapper);

            Router = gameObject.AddComponent<BciInputRouter>();
            Router.Bind(compensation, mock, hybrid);
            Router.RestoreSavedMode();

            Hud = gameObject.AddComponent<AppHudController>();
            Hud.Initialize(Router, Records);
        }
    }
}

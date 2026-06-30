using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// 一键搭建测试场景：BciSystem + OpenSeeFace + 简易演示物体。
    /// 挂到 Bootstrap 场景任意物体上即可运行测试版。
    /// </summary>
    public class BciTestBootstrap : MonoBehaviour
    {
        [Header("OpenSeeFace paths (EasyVtuber)")]
        [SerializeField] private string facetrackerExe =
            @"f:\EasyVtuber\OpenSeeFace-v1.20.4\Binary\facetracker.exe";
        [SerializeField] private string modelsDir =
            @"f:\EasyVtuber\OpenSeeFace-v1.20.4\models\";
        [SerializeField] private int cameraIndex = 0;
        [SerializeField] private bool startTrackerOnPlay = true;
        [SerializeField] private bool useCompensationByDefault = true;

        private BciInputRouter _router;
        private CompensationBciInputProvider _compensation;
        private Transform _demoCube;

        private void Awake()
        {
            BuildBciSystem();
            BuildDemoVisual();
        }

        private void Start()
        {
            if (startTrackerOnPlay && _router.UseCompensation)
                _compensation.StartTracking();
        }

        private void Update()
        {
            if (_demoCube == null || _router == null) return;
            var focus = _router.Current.Focus;
            var speed = Mathf.Lerp(1f, 8f, focus / 100f);
            _demoCube.Rotate(Vector3.up, speed * Time.deltaTime * 30f);
            if (_router.Current.Blink)
                _demoCube.localScale = Vector3.one * 1.3f;
            else
                _demoCube.localScale = Vector3.Lerp(_demoCube.localScale, Vector3.one, Time.deltaTime * 8f);
        }

        private void BuildBciSystem()
        {
            var root = new GameObject("BciSystem");
            var openSee = root.AddComponent<OpenSee.OpenSee>();
            var launcher = root.AddComponent<OpenSee.OpenSeeLauncher>();
            launcher.openSeeTarget = openSee;
            launcher.exePath = facetrackerExe;
            launcher.modelPath = modelsDir;
            launcher.cameraIndex = cameraIndex;
            launcher.autoStart = false;
            launcher.dontPrint = true;

            _compensation = root.AddComponent<CompensationBciInputProvider>();
            var mock = root.AddComponent<MockBciInputProvider>();
            var hybrid = root.AddComponent<HybridBciInputProvider>();
            _compensation.Bind(openSee, launcher);
            _compensation.ConfigurePaths(facetrackerExe, modelsDir, cameraIndex);

            _router = root.AddComponent<BciInputRouter>();
            _router.Bind(_compensation, mock, hybrid);
            _router.UseCompensation = useCompensationByDefault;

            var hud = root.AddComponent<BciTestHud>();
            hud.Bind(_router, _compensation);
        }

        private void BuildDemoVisual()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "FocusDemoCube";
            cube.transform.position = new Vector3(0, 1f, 2f);
            _demoCube = cube.transform;
        }
    }
}

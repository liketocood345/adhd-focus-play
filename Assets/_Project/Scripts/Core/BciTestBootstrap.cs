using UnityEngine;
using UnityEngine.SceneManagement;

namespace ADHDTraining.Core
{
    /// <summary>
    /// 一键搭建测试场景：BciSystem + MediaPipe + 简易演示物体。
    /// 挂到 Bootstrap 场景任意物体上即可运行测试版。
    /// </summary>
    public class BciTestBootstrap : MonoBehaviour
    {
        [Header("MediaPipe")]
        [SerializeField] private string pythonExe = MediaPipe.MediaPipeBridgeLauncher.DefaultPython;
        [SerializeField] private int cameraIndex = 0;
        [SerializeField] private bool startTrackerOnPlay = true;
        [SerializeField] private bool useCompensationByDefault = true;
        [SerializeField] private bool redirectToMainMenu = true;

        private BciInputRouter _router;
        private CompensationBciInputProvider _compensation;
        private Transform _demoCube;

        private void Awake()
        {
            if (redirectToMainMenu)
            {
                if (SceneManager.GetActiveScene().name != SceneNames.MainMenu)
                    SceneManager.LoadScene(SceneNames.MainMenu);
                return;
            }

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
            _compensation = root.AddComponent<CompensationBciInputProvider>();
            var mock = root.AddComponent<MockBciInputProvider>();
            var hybrid = root.AddComponent<HybridBciInputProvider>();
            _compensation.ConfigurePaths(pythonExe, null, cameraIndex);

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

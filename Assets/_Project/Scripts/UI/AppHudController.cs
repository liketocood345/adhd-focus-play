using ADHDTraining.Core;
using ADHDTraining.Core.Session;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ADHDTraining.UI
{
    /// <summary>
    /// 全局 HUD：主菜单显示历史记录与输入模式；游戏中仅调试与暂停提示。
    /// </summary>
    public class AppHudController : MonoBehaviour
    {
        private BciInputRouter _router;
        private SessionRecordService _records;
        private Canvas _canvas;
        private MainMenuRightDock _rightDock;
        private InputModePanel _modePanel;
        private ScoreboardPanel _scoreboard;
        private CompensationVideoSourcePanel _videoSourcePanel;
        private DebugPanel _debugPanel;
        private Text _pauseBanner;

        public void Initialize(BciInputRouter router, SessionRecordService records)
        {
            _router = router;
            _records = records;
            BuildCanvas();
            ApplySceneVisibility(SceneManager.GetActiveScene().name);
        }

        public void RefreshScoreboard() => _scoreboard?.Refresh(_records);

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) =>
            ApplySceneVisibility(scene.name);

        private void ApplySceneVisibility(string sceneName)
        {
            if (sceneName == SceneNames.MainMenu)
                EnsureMainMenuPanels();
            else
                ClearMainMenuPanels();
        }

        private void EnsureMainMenuPanels()
        {
            if (_canvas == null) return;

            if (_rightDock == null)
                _rightDock = MainMenuRightDock.Create(_canvas.transform);

            // 垂直布局自上而下：视频源在上、输入模式贴底
            if (_videoSourcePanel == null && _router?.Compensation != null)
                _videoSourcePanel = CompensationVideoSourcePanel.Create(_rightDock.transform, _router, _router.Compensation);
            if (_modePanel == null)
                _modePanel = InputModePanel.Create(_rightDock.transform, _router);
            if (_scoreboard == null)
                _scoreboard = ScoreboardPanel.Create(_canvas.transform, _records);

            _rightDock.gameObject.SetActive(true);
            _modePanel.gameObject.SetActive(true);
            _scoreboard.gameObject.SetActive(true);
            _videoSourcePanel?.gameObject.SetActive(true);
            RefreshScoreboard();
        }

        private void ClearMainMenuPanels()
        {
            if (_rightDock != null)
            {
                Destroy(_rightDock.gameObject);
                _rightDock = null;
                _modePanel = null;
                _videoSourcePanel = null;
            }

            if (_scoreboard != null)
            {
                Destroy(_scoreboard.gameObject);
                _scoreboard = null;
            }
        }

        private void Update()
        {
            if (_router == null) return;
            _debugPanel?.Refresh(_router);
            if (_pauseBanner != null)
                _pauseBanner.gameObject.SetActive(FindPauseController()?.IsPaused ?? false);
        }

        private FocusPauseController FindPauseController()
        {
            foreach (var c in FindObjectsByType<FocusPauseController>(FindObjectsInactive.Exclude))
                if (c.gameObject.scene.isLoaded) return c;
            return null;
        }

        private void BuildCanvas()
        {
            UiEventSystem.Ensure();

            var canvasGo = new GameObject("AppHudCanvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 500;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _debugPanel = DebugPanel.Create(_canvas.transform, _router);

            _pauseBanner = CreateText(_canvas.transform, "PauseBanner", "专注度过低，已暂停", 28, TextAnchor.MiddleCenter);
            var rt = _pauseBanner.rectTransform;
            rt.anchorMin = new Vector2(0.25f, 0.45f);
            rt.anchorMax = new Vector2(0.75f, 0.55f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            _pauseBanner.color = new Color(1f, 0.85f, 0.2f);
            _pauseBanner.gameObject.SetActive(false);
        }

        internal static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = UiFonts.Default;
            if (text.font == null)
                Debug.LogWarning("[UI] UiFonts.Default is null — text may be invisible.");
            text.supportRichText = true;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            var rt = text.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return text;
        }

        internal static Button CreateButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(label + "Button");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            UiSprites.Apply(img, new Color(0.15f, 0.2f, 0.35f, 0.92f));
            var btn = go.AddComponent<Button>();
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = new Vector2(2, 2);
            rt.offsetMax = new Vector2(-2, -2);
            var labelText = CreateText(go.transform, "Label", label, 16, TextAnchor.MiddleCenter);
            labelText.resizeTextForBestFit = true;
            labelText.resizeTextMinSize = 10;
            labelText.resizeTextMaxSize = 16;
            return btn;
        }
    }
}

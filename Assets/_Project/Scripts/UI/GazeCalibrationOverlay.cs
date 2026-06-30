using UnityEngine;
using UnityEngine.UI;

namespace ADHDTraining.UI
{
    /// <summary>
    /// 全黑校准屏 + 红点 + 提示文字。
    /// </summary>
    public class GazeCalibrationOverlay : MonoBehaviour
    {
        private static GazeCalibrationOverlay _instance;

        private Canvas _canvas;
        private RectTransform _dot;
        private Text _instruction;

        public static GazeCalibrationOverlay Ensure()
        {
            if (_instance != null) return _instance;
            var go = new GameObject("GazeCalibrationOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<GazeCalibrationOverlay>();
            _instance.Build();
            return _instance;
        }

        public void Show()
        {
            if (_canvas != null) _canvas.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (_canvas != null) _canvas.gameObject.SetActive(false);
        }

        public void SetTarget(float normalizedX, float normalizedY)
        {
            if (_dot == null) return;
            _dot.anchorMin = _dot.anchorMax = new Vector2(normalizedX, normalizedY);
            _dot.anchoredPosition = Vector2.zero;
        }

        public void SetInstruction(string text)
        {
            if (_instruction != null) _instruction.text = text;
        }

        private void Build()
        {
            UiEventSystem.Ensure();

            var canvasGo = new GameObject("Canvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 4000;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var bg = new GameObject("Background");
            bg.transform.SetParent(canvasGo.transform, false);
            var bgImg = bg.AddComponent<Image>();
            UiSprites.Apply(bgImg, Color.black);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;

            var dotGo = new GameObject("RedDot");
            dotGo.transform.SetParent(canvasGo.transform, false);
            _dot = dotGo.AddComponent<RectTransform>();
            _dot.sizeDelta = new Vector2(28, 28);
            var dotImg = dotGo.AddComponent<Image>();
            UiSprites.Apply(dotImg, new Color(1f, 0.12f, 0.1f, 1f));

            var textGo = new GameObject("Instruction");
            textGo.transform.SetParent(canvasGo.transform, false);
            var trt = textGo.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.15f, 0.62f);
            trt.anchorMax = new Vector2(0.85f, 0.88f);
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            _instruction = AppHudController.CreateText(textGo.transform, "Text", "", 28, TextAnchor.MiddleCenter);
            _instruction.color = Color.white;

            _canvas.gameObject.SetActive(false);
        }
    }
}

using ADHDTraining.Core;
using ADHDTraining.Core.Session;
using UnityEngine;
using UnityEngine.UI;

namespace ADHDTraining.UI
{
    [DefaultExecutionOrder(-50)]
    public class MainMenuController : MonoBehaviour
    {
        private void Awake()
        {
            UiEventSystem.Ensure();
            BuildMenu();
            AppRoot.Ensure();
        }

        private void BuildMenu()
        {
            if (GameObject.Find("MainMenuCanvas") != null) return;

            var canvas = UiCanvasFactory.CreateOverlay("MainMenuCanvas", 100);

            UiCanvasFactory.CreatePanel(
                canvas.transform, "Background",
                new Color(0.06f, 0.09f, 0.14f, 0.95f),
                Vector2.zero, Vector2.one);

            var title = AppHudController.CreateText(canvas.transform, "Title", "ADHD 注意力训练", 42, TextAnchor.UpperCenter);
            title.rectTransform.anchorMin = new Vector2(0.2f, 0.78f);
            title.rectTransform.anchorMax = new Vector2(0.8f, 0.95f);
            title.rectTransform.offsetMin = title.rectTransform.offsetMax = Vector2.zero;
            title.color = new Color(0.9f, 0.95f, 1f);

            CreateGameButton(canvas.transform, "听音寻宝 · 选择性注意", GameIds.Selective, 0.72f);
            CreateGameButton(canvas.transform, "无尽跑酷者 · 持续性注意", GameIds.Sustained, 0.62f);
            CreateGameButton(canvas.transform, "指令反转 · 注意力转移", GameIds.Shifting, 0.52f);
            CreateGameButton(canvas.transform, "双线救援 · 注意力分配", GameIds.Divided, 0.42f);
            CreateGameButton(canvas.transform, "红灯停绿灯行 · 注意力抑制", GameIds.Inhibition, 0.32f);
        }

        private static void CreateGameButton(Transform parent, string label, string gameId, float yAnchor)
        {
            var btn = AppHudController.CreateButton(
                parent, label,
                new Vector2(MainMenuHudLayout.GameButtonMinX, yAnchor - 0.055f),
                new Vector2(MainMenuHudLayout.GameButtonMaxX, yAnchor));
            btn.onClick.AddListener(() => SceneLoader.LoadGame(gameId));
        }
    }
}

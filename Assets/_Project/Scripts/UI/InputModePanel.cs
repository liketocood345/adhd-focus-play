using ADHDTraining.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ADHDTraining.UI
{
    public class InputModePanel : MonoBehaviour
    {
        private BciInputRouter _router;

        public static InputModePanel Create(Transform parent, BciInputRouter router)
        {
            var root = new GameObject("InputModePanel");
            root.transform.SetParent(parent, false);
            var panel = root.AddComponent<InputModePanel>();
            panel._router = router;
            panel.Build();
            return panel;
        }

        private void Build()
        {
            var rt = gameObject.AddComponent<RectTransform>();
            MainMenuRightDock.ApplyChildLayout(gameObject, MainMenuHudLayout.InputModeHeight);

            var bg = gameObject.AddComponent<Image>();
            UiSprites.Apply(bg, new Color(0.05f, 0.08f, 0.12f, 0.88f));

            var title = AppHudController.CreateText(transform, "Title", "输入模式", 15, TextAnchor.MiddleLeft);
            var titleRt = title.rectTransform;
            titleRt.anchorMin = new Vector2(0.04f, 0.62f);
            titleRt.anchorMax = new Vector2(0.96f, 0.96f);
            titleRt.offsetMin = titleRt.offsetMax = Vector2.zero;

            CreateModeButton("代偿", BciInputMode.Compensation, new Vector2(0.02f, 0.06f), new Vector2(0.32f, 0.58f));
            CreateModeButton("脑机", BciInputMode.HybridBci, new Vector2(0.34f, 0.06f), new Vector2(0.64f, 0.58f));
            CreateModeButton("键鼠", BciInputMode.Mock, new Vector2(0.66f, 0.06f), new Vector2(0.98f, 0.58f));
        }

        private void CreateModeButton(string label, BciInputMode mode, Vector2 min, Vector2 max)
        {
            var btn = AppHudController.CreateButton(transform, label, min, max);
            btn.onClick.AddListener(() =>
            {
                _router.SetInputMode(mode);
                Highlight(mode);
            });
            if (_router.ActiveMode == mode) Highlight(mode);
        }

        private void Highlight(BciInputMode mode)
        {
            // visual feedback via button colors handled simply by re-select on click
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace ADHDTraining.UI
{
    /// <summary>
    /// 主菜单右下角垂直停靠区：输入模式 + 代偿视频源，避免面板互相挤压。
    /// </summary>
    public class MainMenuRightDock : MonoBehaviour
    {
        public const float DockWidth = 340f;
        public const float EdgeMargin = 12f;

        public static MainMenuRightDock Create(Transform parent)
        {
            var root = new GameObject("MainMenuRightDock");
            root.transform.SetParent(parent, false);

            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-EdgeMargin, EdgeMargin);
            rt.sizeDelta = new Vector2(DockWidth, 0f);

            var fitter = root.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.LowerRight;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = MainMenuHudLayout.PanelGap;
            layout.padding = new RectOffset(0, 0, 0, 0);

            return root.AddComponent<MainMenuRightDock>();
        }

        public static void ApplyChildLayout(GameObject child, float preferredHeight)
        {
            var le = child.GetComponent<LayoutElement>();
            if (le == null) le = child.AddComponent<LayoutElement>();
            le.preferredWidth = DockWidth;
            le.preferredHeight = preferredHeight;
            le.minHeight = preferredHeight;
        }
    }
}

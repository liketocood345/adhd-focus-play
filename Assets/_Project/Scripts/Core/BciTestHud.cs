using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// 测试版 HUD：显示 BCI 状态与操作提示。
    /// </summary>
    public class BciTestHud : MonoBehaviour
    {
        [SerializeField] private BciInputRouter router;
        [SerializeField] private CompensationBciInputProvider compensation;
        [SerializeField] private KeyCode toggleTrackerKey = KeyCode.T;

        private bool _trackerRunning;

        public void Bind(BciInputRouter inputRouter, CompensationBciInputProvider compensationProvider)
        {
            router = inputRouter;
            compensation = compensationProvider;
        }

        private void OnGUI()
        {
            if (router == null) return;

            var snap = router.Current;
            var mode = router.UseCompensation ? "代偿 (摄像头+OpenSeeFace)" : router.ActiveMode.ToString();

            GUILayout.BeginArea(new Rect(10, 10, 420, 280), GUI.skin.box);
            GUILayout.Label("<b>ADHD 注意力训练 — BCI 测试版</b>");
            GUILayout.Label($"模式: {mode}");
            GUILayout.Label($"连接: {(router.IsConnected ? "是" : "否")}");
            GUILayout.Label($"专注力: {snap.Focus:F1}");
            GUILayout.Label($"眨眼(本帧): {snap.Blink}");
            GUILayout.Label($"头动: {snap.Head}");
            GUILayout.Space(8);
            GUILayout.Label("滚轮 — 调节专注力");
            GUILayout.Label("C — 切换代偿开/关");
            GUILayout.Label("T — 启动/停止 OpenSeeFace");
            GUILayout.Label("模拟模式: 空格眨眼, W/S/Q/E 头动");
            GUILayout.EndArea();
        }

        private void Update()
        {
            if (router == null || compensation == null) return;
            if (!Input.GetKeyDown(toggleTrackerKey) || !router.UseCompensation)
                return;

            if (_trackerRunning)
            {
                compensation.StopTracking();
                _trackerRunning = false;
            }
            else
            {
                compensation.StartTracking();
                _trackerRunning = true;
            }
        }
    }
}

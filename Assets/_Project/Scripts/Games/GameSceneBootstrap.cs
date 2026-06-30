using ADHDTraining.Core;
using ADHDTraining.Core.Session;
using ADHDTraining.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ADHDTraining.Games
{
    /// <summary>
    /// 各游戏场景通用：相机、灯光、返回按钮、暂停提示。
    /// </summary>
    public class GameSceneBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            AppRoot.Ensure();
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.12f, 0.14f, 0.2f);
                camGo.AddComponent<AudioListener>();
                camGo.transform.position = new Vector3(0, 4f, -8f);
                camGo.transform.rotation = Quaternion.Euler(18f, 0, 0);
            }

            if (Object.FindFirstObjectByType<Light>() == null)
            {
                var lightGo = new GameObject("Directional Light");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0);
            }

            BuildReturnButton();
        }

        private void BuildReturnButton()
        {
            var canvas = UiCanvasFactory.CreateOverlay("GameOverlay", 200);

            var btn = AppHudController.CreateButton(canvas.transform, "返回主菜单", new Vector2(0.78f, 0.9f), new Vector2(0.98f, 0.98f));
            btn.onClick.AddListener(() =>
            {
                var session = Object.FindFirstObjectByType<GameSessionBase>();
                if (session != null) session.ReturnToMainMenu();
                else SceneLoader.LoadMainMenu();
            });
        }
    }
}

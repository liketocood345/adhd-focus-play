#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ADHDTraining.Editor
{
    /// <summary>
    /// 无论从哪个场景点 Play，都从主菜单进入（避免误开 Bootstrap 看不到 UI）。
    /// </summary>
    [InitializeOnLoad]
    public static class EditorPlayModeEntry
    {
        private const string MainMenuPath = "Assets/_Project/Scenes/MainMenu.unity";

        static EditorPlayModeEntry()
        {
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuPath);
            if (scene != null)
                EditorSceneManager.playModeStartScene = scene;
            else
                Debug.LogWarning($"[ADHD Training] Play 入口场景未找到: {MainMenuPath}");
        }
    }
}
#endif

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using ADHDTraining.Core;
using ADHDTraining.Games;
using ADHDTraining.Games.Divided;
using ADHDTraining.Games.Inhibition;
using ADHDTraining.Games.Selective;
using ADHDTraining.Games.Shifting;
using ADHDTraining.Games.Sustained;
using ADHDTraining.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ADHDTraining.Editor
{
    public static class ProjectSceneSetup
    {
        private const string ScenesRoot = "Assets/_Project/Scenes";
        private const string GamesRoot = "Assets/_Project/Scenes/Games";

        [MenuItem("ADHD Training/Setup All Scenes And Build Settings")]
        public static void SetupAll()
        {
            Directory.CreateDirectory(ScenesRoot);
            Directory.CreateDirectory(GamesRoot);

            CreateMainMenuScene();
            CreateGameScene(SceneNames.GameSelective, typeof(SelectiveGameSession));
            CreateGameScene(SceneNames.GameSustained, typeof(SustainedGameSession));
            CreateGameScene(SceneNames.GameShifting, typeof(ShiftingGameSession));
            CreateGameScene(SceneNames.GameDivided, typeof(DividedGameSession));
            CreateGameScene(SceneNames.GameInhibition, typeof(InhibitionGameSession));

            ApplyBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ADHD Training] Scenes and Build Settings updated.");
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var bootstrap = new GameObject("MainMenu");
            bootstrap.AddComponent<MainMenuController>();

            SaveScene(scene, $"{ScenesRoot}/MainMenu.unity");
        }

        private static void CreateGameScene(string sceneName, System.Type sessionType)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("GameRoot");
            root.AddComponent<GameSceneBootstrap>();
            root.AddComponent(sessionType);

            SaveScene(scene, $"{GamesRoot}/{sceneName}.unity");
        }

        private static void SaveScene(Scene scene, string path)
        {
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void ApplyBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                SceneEntry($"{ScenesRoot}/MainMenu.unity"),
                SceneEntry($"{GamesRoot}/Game_Selective.unity"),
                SceneEntry($"{GamesRoot}/Game_Sustained.unity"),
                SceneEntry($"{GamesRoot}/Game_Shifting.unity"),
                SceneEntry($"{GamesRoot}/Game_Divided.unity"),
                SceneEntry($"{GamesRoot}/Game_Inhibition.unity"),
                SceneEntry($"{ScenesRoot}/Bootstrap.unity", enabled: false)
            };

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static EditorBuildSettingsScene SceneEntry(string path, bool enabled = true)
        {
            return new EditorBuildSettingsScene(path, enabled);
        }
    }
}
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using ADHDTraining.Core.Session;

namespace ADHDTraining.Core
{
    public static class SceneLoader
    {
        public static void LoadMainMenu() => SceneManager.LoadScene(SceneNames.MainMenu);

        public static void LoadGame(string gameId)
        {
            var scene = GameIds.SceneName(gameId);
            SceneManager.LoadScene(scene);
        }
    }
}

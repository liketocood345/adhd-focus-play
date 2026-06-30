#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ADHDTraining.Editor
{
    public static class OpenSourceArtImporter
    {
        private const string ManifestRel = "Tools/import_oss_art/manifest.json";
        private const string ImportScriptRel = "Tools/import_oss_art/import.ps1";
        private const string ResourcesArtRoot = "Assets/_Project/Resources/Art/ThirdParty";
        private const string BindingsAsset = "Assets/_Project/Resources/art_bindings.json";

        [MenuItem("ADHD Training/Import Open Source Art")]
        public static void ImportArt()
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var script = Path.Combine(projectRoot, ImportScriptRel.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(script))
            {
                EditorUtility.DisplayDialog("Import Open Source Art", $"Script not found:\n{script}", "OK");
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{script}\"",
                WorkingDirectory = projectRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            var output = process.StandardOutput.ReadToEnd();
            var err = process.StandardError.ReadToEnd();
            process.WaitForExit();

            UnityEngine.Debug.Log(output);
            if (!string.IsNullOrEmpty(err))
                UnityEngine.Debug.LogWarning(err);

            AssetDatabase.Refresh();
            LinkArtBindings();

            EditorUtility.DisplayDialog(
                "Import Open Source Art",
                process.ExitCode == 0
                    ? "美术素材已复制到 Resources/Art/ThirdParty。\n已尝试自动链接 art_bindings.json。"
                    : $"导入结束，退出码 {process.ExitCode}。请查看 Console。",
                "OK");
        }

        [MenuItem("ADHD Training/Link Art Resource Bindings")]
        public static void LinkArtBindings()
        {
            var manifest = LoadManifest();
            if (manifest?.bindingEntries == null || manifest.bindingEntries.Length == 0)
            {
                UnityEngine.Debug.LogWarning("[ADHD Training] manifest bindingEntries missing.");
                return;
            }

            var games = new List<GameBindingDto>();
            foreach (var entry in manifest.bindingEntries)
            {
                var repoFolder = $"{ResourcesArtRoot}/{entry.repo}";
                if (!AssetDatabase.IsValidFolder(repoFolder))
                {
                    UnityEngine.Debug.LogWarning($"[ADHD Training] Repo art folder missing: {repoFolder}");
                    continue;
                }

                var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { repoFolder });
                var prefabs = prefabGuids
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(p => (path: p, name: Path.GetFileNameWithoutExtension(p)))
                    .ToList();

                var bindings = new List<PrefabBindingDto>();
                foreach (var slot in entry.slots)
                {
                    if (slot?.hints == null || slot.hints.Length == 0) continue;
                    var match = prefabs.FirstOrDefault(p => slot.hints.Any(hint =>
                        p.name.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0));

                    if (string.IsNullOrEmpty(match.path))
                    {
                        UnityEngine.Debug.LogWarning($"[ADHD Training] No prefab for {entry.gameId}.{slot.slot} in {entry.repo}");
                        continue;
                    }

                    var resourcePath = ToResourcesPath(match.path);
                    bindings.Add(new PrefabBindingDto { slot = slot.slot, path = resourcePath });
                    UnityEngine.Debug.Log($"[ADHD Training] {entry.gameId}.{slot.slot} -> {resourcePath}");
                }

                if (bindings.Count > 0)
                    games.Add(new GameBindingDto { id = entry.gameId, prefabs = bindings.ToArray() });
            }

            var json = JsonUtility.ToJson(new ArtBindingsFileDto { games = games.ToArray() }, true);
            File.WriteAllText(BindingsAsset, json);
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("[ADHD Training] art_bindings.json updated.");
        }

        private static string ToResourcesPath(string assetPath)
        {
            const string marker = "/Resources/";
            var idx = assetPath.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0) return assetPath;
            var rest = assetPath.Substring(idx + marker.Length);
            var dot = rest.LastIndexOf('.');
            return dot > 0 ? rest.Substring(0, dot) : rest;
        }

        private static ManifestDto LoadManifest()
        {
            var path = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                ManifestRel.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path)) return null;
            return JsonUtility.FromJson<ManifestDto>(File.ReadAllText(path));
        }

        [Serializable]
        private class ManifestDto
        {
            public BindingEntryDto[] bindingEntries;
        }

        [Serializable]
        private class BindingEntryDto
        {
            public string gameId;
            public string repo;
            public SlotHintDto[] slots;
        }

        [Serializable]
        private class SlotHintDto
        {
            public string slot;
            public string[] hints;
        }

        [Serializable]
        private class ArtBindingsFileDto
        {
            public GameBindingDto[] games;
        }

        [Serializable]
        private class GameBindingDto
        {
            public string id;
            public PrefabBindingDto[] prefabs;
        }

        [Serializable]
        private class PrefabBindingDto
        {
            public string slot;
            public string path;
        }
    }
}
#endif

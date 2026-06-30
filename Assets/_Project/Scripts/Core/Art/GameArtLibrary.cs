using System;
using System.Collections.Generic;
using UnityEngine;

namespace ADHDTraining.Core.Art
{
    /// <summary>
    /// 从 Resources/Art/ThirdParty 加载开源项目美术；缺失时由调用方回退 Primitive。
    /// </summary>
    public static class GameArtLibrary
    {
        private const string BindingsPath = "art_bindings";
        private static Dictionary<string, Dictionary<string, string>> _paths;
        private static readonly Dictionary<string, Dictionary<string, GameObject>> PrefabCache = new();

        public static GameObject Instantiate(string gameId, string slot, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var prefab = LoadPrefab(gameId, slot);
            if (prefab == null) return null;

            var go = UnityEngine.Object.Instantiate(prefab, position, rotation);
            go.name = prefab.name;
            go.transform.localScale = scale;
            return go;
        }

        public static GameObject LoadPrefab(string gameId, string slot)
        {
            if (PrefabCache.TryGetValue(gameId, out var slots) && slots.TryGetValue(slot, out var cached))
                return cached;

            var path = GetPath(gameId, slot);
            var prefab = !string.IsNullOrEmpty(path) ? Resources.Load<GameObject>(path) : null;
            if (prefab == null)
                prefab = FindPrefabByName(gameId, slot);

            if (prefab != null)
            {
                if (!PrefabCache.ContainsKey(gameId))
                    PrefabCache[gameId] = new Dictionary<string, GameObject>();
                PrefabCache[gameId][slot] = prefab;
            }

            return prefab;
        }

        public static Material LoadMaterial(string gameId, string slot)
        {
            var path = GetPath(gameId, slot);
            return string.IsNullOrEmpty(path) ? null : Resources.Load<Material>(path);
        }

        public static Sprite LoadSprite(string gameId, string slot)
        {
            var path = GetPath(gameId, slot);
            return string.IsNullOrEmpty(path) ? null : Resources.Load<Sprite>(path);
        }

        public static void ApplyMaterialOrColor(Renderer renderer, string gameId, string slot, Color fallback)
        {
            if (renderer == null) return;
            var mat = LoadMaterial(gameId, slot);
            if (mat != null)
            {
                renderer.sharedMaterial = mat;
                return;
            }

            renderer.material.color = fallback;
        }

        private static string GetPath(string gameId, string slot)
        {
            EnsureBindings();
            return _paths.TryGetValue(gameId, out var slots) && slots.TryGetValue(slot, out var path) ? path : null;
        }

        private static GameObject FindPrefabByName(string gameId, string slot)
        {
            var folder = GetRepoFolder(gameId);
            if (string.IsNullOrEmpty(folder)) return null;

            var all = Resources.LoadAll<GameObject>($"Art/ThirdParty/{folder}");
            if (all == null || all.Length == 0) return null;

            foreach (var hint in GetNameHints(gameId, slot))
            {
                foreach (var p in all)
                {
                    if (p.name.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0)
                        return p;
                }
            }

            return null;
        }

        private static string GetRepoFolder(string gameId)
        {
            var path = GetPath(gameId, "player")
                       ?? GetPath(gameId, "collector")
                       ?? GetPath(gameId, "crystal");
            if (string.IsNullOrEmpty(path)) return null;

            const string prefix = "Art/ThirdParty/";
            if (!path.StartsWith(prefix, StringComparison.Ordinal)) return null;
            var rest = path.Substring(prefix.Length);
            var slash = rest.IndexOf('/');
            return slash > 0 ? rest.Substring(0, slash) : rest;
        }

        private static IEnumerable<string> GetNameHints(string gameId, string slot)
        {
            yield return slot;
            switch (gameId)
            {
                case "sustained" when slot == "player":
                    yield return "runner"; yield return "character"; break;
                case "sustained" when slot == "obstacle":
                    yield return "barrier"; yield return "block"; break;
                case "sustained" when slot == "bonus":
                    yield return "coin"; yield return "gem"; yield return "star"; break;
                case "inhibition" when slot == "crystal":
                    yield return "gem"; yield return "fruit"; yield return "good"; break;
                case "inhibition" when slot == "meteor":
                    yield return "bomb"; yield return "bad"; yield return "rock"; break;
            }
        }

        private static void EnsureBindings()
        {
            if (_paths != null) return;
            _paths = new Dictionary<string, Dictionary<string, string>>();

            var text = Resources.Load<TextAsset>(BindingsPath);
            if (text == null) return;

            var data = JsonUtility.FromJson<ArtBindingsData>(text.text);
            if (data?.games == null) return;

            foreach (var game in data.games)
            {
                if (string.IsNullOrEmpty(game.id) || game.prefabs == null) continue;
                var map = new Dictionary<string, string>();
                foreach (var p in game.prefabs)
                {
                    if (!string.IsNullOrEmpty(p.slot) && !string.IsNullOrEmpty(p.path))
                        map[p.slot] = p.path;
                }
                _paths[game.id] = map;
            }
        }

        [Serializable]
        private class ArtBindingsData
        {
            public GameArtEntry[] games;
        }

        [Serializable]
        private class GameArtEntry
        {
            public string id;
            public PrefabBinding[] prefabs;
        }

        [Serializable]
        private class PrefabBinding
        {
            public string slot;
            public string path;
        }
    }
}

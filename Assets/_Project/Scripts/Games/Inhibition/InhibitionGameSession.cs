using ADHDTraining.Core;
using ADHDTraining.Core.Art;
using ADHDTraining.Core.Session;using ADHDTraining.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ADHDTraining.Games.Inhibition
{
    public class InhibitionGameSession : GameSessionBase
    {
        private Text _hudText;
        private int _laserUses = 3;
        private FallingObjectSpawner _spawner;

        protected override void Awake()
        {
            gameId = GameIds.Inhibition;
            sessionDurationSec = 60f;
            base.Awake();
            BuildWorld();
            BuildHud();
        }

        protected override void OnSessionStarted()
        {
            _laserUses = 3;
            _spawner.Begin(this, Input);
        }

        protected override void OnSessionTick()
        {
            if (Input.Current.Head == HeadGesture.Shake && _laserUses > 0)
            {
                _laserUses--;
                _spawner.ClearMeteors();
            }

            _hudText.text = $"红灯停绿灯行\n得分: {Score}  激光: {_laserUses}/3\n剩余: {Remaining:F0}s\n晶石眨眼收集 / 陨石勿眨";
        }

        protected override void OnSessionEnded() => _spawner.Stop();

        protected override string BuildExtraJson() => $"{{\"laser_used\":{3 - _laserUses}}}";

        public void OnFallingResolved(bool crystal, bool blinked)
        {
            if (crystal)
            {
                if (blinked) AddScore(10, true);
            }
            else if (blinked)
            {
                AddScore(-5, false);
            }
        }

        public float FallSpeed => 3f * Mathf.Lerp(0.5f, 1.5f, Input.Current.Focus / 100f);

        private void BuildWorld()
        {
            var player = GameArtLibrary.Instantiate(
                GameIds.Inhibition, "collector", new Vector3(0, 0.5f, 0), Quaternion.identity, new Vector3(1.2f, 0.3f, 1.2f));
            if (player == null)
            {
                player = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                player.name = "Collector";
                player.transform.position = new Vector3(0, 0.5f, 0);
                player.transform.localScale = new Vector3(1.2f, 0.3f, 1.2f);
                player.GetComponent<Renderer>().material.color = new Color(0.3f, 0.8f, 0.5f);
            }
            else
            {
                player.name = "Collector";
            }

            var col = player.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            var spawnerGo = new GameObject("FallingSpawner");
            _spawner = spawnerGo.AddComponent<FallingObjectSpawner>();
        }

        private void BuildHud()
        {
            var canvas = UiCanvasFactory.CreateOverlay("GameHud", 150);
            _hudText = AppHudController.CreateText(canvas.transform, "Hud", "", 22, TextAnchor.UpperCenter);
            var rt = _hudText.rectTransform;
            rt.anchorMin = new Vector2(0.2f, 0.55f);
            rt.anchorMax = new Vector2(0.8f, 0.88f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }

    public class FallingObjectSpawner : MonoBehaviour
    {
        private InhibitionGameSession _session;
        private IBciInputProvider _input;
        private float _nextSpawn;
        private readonly System.Collections.Generic.List<FallingItem> _items = new();

        public void Begin(InhibitionGameSession session, IBciInputProvider input)
        {
            _session = session;
            _input = input;
            _nextSpawn = Time.time + 0.5f;
        }

        public void Stop()
        {
            foreach (var item in _items)
                if (item != null && item.Go != null) Destroy(item.Go);
            _items.Clear();
        }

        public void ClearMeteors()
        {
            for (var i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i].IsCrystal) continue;
                Destroy(_items[i].Go);
                _items.RemoveAt(i);
            }
        }

        private void Update()
        {
            if (_session == null || _input == null) return;
            if (Time.time >= _nextSpawn)
            {
                Spawn(Random.value > 0.35f);
                _nextSpawn = Time.time + Random.Range(0.6f, 1.4f);
            }

            for (var i = _items.Count - 1; i >= 0; i--)
            {
                var item = _items[i];
                item.Go.transform.position += Vector3.down * (_session.FallSpeed * Time.deltaTime);
                if (item.Go.transform.position.y < 0.6f)
                {
                    _session.OnFallingResolved(item.IsCrystal, _input.Current.Blink);
                    Destroy(item.Go);
                    _items.RemoveAt(i);
                }
            }
        }

        private void Spawn(bool crystal)
        {
            var slot = crystal ? "crystal" : "meteor";
            var pos = new Vector3(Random.Range(-3f, 3f), 6f, 0);
            var scale = Vector3.one * 0.6f;
            var go = GameArtLibrary.Instantiate(GameIds.Inhibition, slot, pos, Quaternion.identity, scale);
            if (go == null)
            {
                go = GameObject.CreatePrimitive(crystal ? PrimitiveType.Sphere : PrimitiveType.Cube);
                go.transform.position = pos;
                go.transform.localScale = scale;
                go.GetComponent<Renderer>().material.color = crystal ? new Color(0.4f, 1f, 0.6f) : new Color(1f, 0.35f, 0.2f);
            }

            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            _items.Add(new FallingItem { Go = go, IsCrystal = crystal });
        }

        private class FallingItem
        {
            public GameObject Go;
            public bool IsCrystal;
        }
    }
}

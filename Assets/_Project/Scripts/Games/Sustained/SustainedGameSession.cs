using ADHDTraining.Core;
using ADHDTraining.Core.Art;
using ADHDTraining.Core.Session;using ADHDTraining.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ADHDTraining.Games.Sustained
{
    public class SustainedGameSession : GameSessionBase
    {
        private Transform _player;
        private RunnerObstacleSpawner _spawner;
        private Text _hudText;
        private int _lane;
        private float _highFocusStreak;
        private int _dashCount;

        protected override void Awake()
        {
            gameId = GameIds.Sustained;
            sessionDurationSec = 60f;
            base.Awake();
            BuildWorld();
            BuildHud();
        }

        protected override void OnSessionStarted()
        {
            _lane = 0;
            _dashCount = 0;
            _highFocusStreak = 0f;
            _spawner.ResetRun();
        }

        protected override void OnSessionTick()
        {
            if (_player == null) return;
            var focus = Input.Current.Focus;
            var speedMul = Mathf.Lerp(0.4f, 1.5f, focus / 100f);
            if (focus > 70f) _highFocusStreak += Time.deltaTime;
            else _highFocusStreak = 0f;
            if (_highFocusStreak > 5f) speedMul *= 1.35f;

            _player.position += Vector3.forward * (6f * speedMul * Time.deltaTime);

            if (Input.Current.Head == HeadGesture.TurnLeft && _lane > -1) _lane--;
            if (Input.Current.Head == HeadGesture.TurnRight && _lane < 1) _lane++;
            var targetX = _lane * 2.2f;
            var pos = _player.position;
            pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * 8f);
            _player.position = pos;

            _hudText.text = $"无尽跑酷者\n专注: {focus:F0}  速度x{speedMul:F1}\n得分: {Score}\n剩余: {Remaining:F0}s";
        }

        protected override void OnSessionEnded() { }

        protected override string BuildExtraJson() => $"{{\"dash_count\":{_dashCount}}}";

        private void BuildWorld()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.transform.localScale = new Vector3(2f, 1f, 8f);
            ground.GetComponent<Renderer>().material.color = new Color(0.25f, 0.35f, 0.3f);

            var playerGo = GameArtLibrary.Instantiate(
                GameIds.Sustained, "player", new Vector3(0, 1f, 0), Quaternion.identity, Vector3.one);
            if (playerGo == null)
            {
                playerGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                playerGo.name = "Player";
                playerGo.transform.position = new Vector3(0, 1f, 0);
                playerGo.GetComponent<Renderer>().material.color = new Color(0.2f, 0.6f, 1f);
            }
            else
            {
                playerGo.name = "Player";
            }
            var col = playerGo.GetComponent<Collider>();
            col.isTrigger = true;
            var rb = playerGo.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            playerGo.AddComponent<RunnerPlayerCollider>().Init(this);
            _player = playerGo.transform;

            var spawnerGo = new GameObject("ObstacleSpawner");
            _spawner = spawnerGo.AddComponent<RunnerObstacleSpawner>();
            _spawner.Init(_player);
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

        public void OnObstacleHit(bool bonus)
        {
            if (bonus) AddScore(15, true);
            else AddScore(-8, false);
        }
    }

    public class RunnerPlayerCollider : MonoBehaviour
    {
        private SustainedGameSession _session;

        public void Init(SustainedGameSession session) => _session = session;

        private void OnTriggerEnter(Collider other)
        {
            if (_session == null) return;
            if (other.CompareTag("Untagged") && other.name.Contains("Obstacle"))
            {
                _session.OnObstacleHit(other.name.Contains("Bonus"));
                Destroy(other.gameObject);
            }
        }
    }

    public class RunnerObstacleSpawner : MonoBehaviour
    {
        private Transform _player;
        private float _nextSpawn;

        public void Init(Transform player) => _player = player;

        public void ResetRun() => _nextSpawn = Time.time + 1f;

        private void Update()
        {
            if (_player == null || Time.time < _nextSpawn) return;
            Spawn(Random.Range(-1, 2));
            _nextSpawn = Time.time + Random.Range(0.8f, 1.6f);
        }

        private void Spawn(int lane)
        {
            var bonus = Random.value > 0.75f;
            var slot = bonus ? "bonus" : "obstacle";
            var scale = Vector3.one * (bonus ? 0.8f : 1.2f);
            var pos = new Vector3(lane * 2.2f, 1f, _player.position.z + 25f);

            var go = GameArtLibrary.Instantiate(GameIds.Sustained, slot, pos, Quaternion.identity, scale);
            if (go == null)
            {
                go = GameObject.CreatePrimitive(bonus ? PrimitiveType.Sphere : PrimitiveType.Cube);
                go.name = bonus ? "Obstacle_Bonus" : "Obstacle_Hazard";
                go.transform.position = pos;
                go.transform.localScale = scale;
                go.GetComponent<Renderer>().material.color = bonus ? Color.yellow : Color.red;
            }
            else
            {
                go.name = bonus ? "Obstacle_Bonus" : "Obstacle_Hazard";
            }

            var col = go.GetComponent<Collider>() ?? go.AddComponent<BoxCollider>();
            col.isTrigger = true;
        }
    }
}

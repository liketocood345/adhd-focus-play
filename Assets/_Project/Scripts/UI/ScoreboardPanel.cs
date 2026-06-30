using System.Collections.Generic;
using System.Linq;
using ADHDTraining.Core.Session;
using UnityEngine;
using UnityEngine.UI;

namespace ADHDTraining.UI
{
    public class ScoreboardPanel : MonoBehaviour
    {
        private Text _summaryText;
        private SessionRecordService _records;
        private bool _expanded;

        public static ScoreboardPanel Create(Transform parent, SessionRecordService records)
        {
            var root = new GameObject("ScoreboardPanel");
            root.transform.SetParent(parent, false);
            var panel = root.AddComponent<ScoreboardPanel>();
            panel._records = records;
            panel.Build();
            return panel;
        }

        public void Refresh(SessionRecordService records)
        {
            _records = records;
            if (_summaryText == null) return;
            var all = records.LoadAll();
            if (all.Count == 0)
            {
                _summaryText.text = "历史记录：暂无";
                return;
            }

            var groups = all.GroupBy(r => r.GameId);
            var lines = new List<string> { "历史计分" };
            foreach (var g in groups)
            {
                var best = g.Max(r => r.Score);
                var last = g.OrderByDescending(r => r.TimestampUtc).First();
                lines.Add($"{g.First().GameNameCn}: {g.Count()}局 最高{best} 最近{last.Score}分");
            }
            if (_expanded)
            {
                foreach (var r in all.OrderByDescending(x => x.TimestampUtc).Take(8))
                    lines.Add($"· {r.GameNameCn} {r.Score}分 {r.TimestampUtc.Substring(0, 16)}");
            }
            _summaryText.text = string.Join("\n", lines);
        }

        private void Build()
        {
            var rt = gameObject.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.sizeDelta = new Vector2(420, _expanded ? 280 : 140);
            rt.anchoredPosition = new Vector2(12, 12);

            var bg = gameObject.AddComponent<Image>();
            UiSprites.Apply(bg, new Color(0.05f, 0.08f, 0.12f, 0.88f));

            _summaryText = AppHudController.CreateText(transform, "Summary", "历史记录：加载中…", 15, TextAnchor.UpperLeft);
            var trt = _summaryText.rectTransform;
            trt.anchorMin = new Vector2(0.02f, 0.15f);
            trt.anchorMax = new Vector2(0.98f, 0.95f);
            trt.offsetMin = trt.offsetMax = Vector2.zero;

            var toggle = AppHudController.CreateButton(transform, _expanded ? "收起" : "展开", new Vector2(0.02f, 0.02f), new Vector2(0.3f, 0.12f));
            toggle.onClick.AddListener(() =>
            {
                _expanded = !_expanded;
                GetComponent<RectTransform>().sizeDelta = new Vector2(420, _expanded ? 280 : 140);
                Refresh(_records);
            });
        }
    }
}

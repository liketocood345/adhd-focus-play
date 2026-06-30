using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ADHDTraining.Core.Session
{
    public class SessionRecordService : MonoBehaviour
    {
        public string CsvPath => Path.Combine(Application.persistentDataPath, "session_records.csv");

        public void Append(GameSessionRecord record)
        {
            var dir = Path.GetDirectoryName(CsvPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var writeHeader = !File.Exists(CsvPath);
            using var writer = new StreamWriter(CsvPath, append: true);
            if (writeHeader)
                writer.WriteLine(GameSessionRecord.CsvHeader);
            writer.WriteLine(record.ToCsvLine());
        }

        public List<GameSessionRecord> LoadAll()
        {
            var list = new List<GameSessionRecord>();
            if (!File.Exists(CsvPath)) return list;

            var lines = File.ReadAllLines(CsvPath);
            for (var i = 1; i < lines.Length; i++)
            {
                var row = ParseLine(lines[i]);
                if (row != null) list.Add(row);
            }
            return list;
        }

        public static GameSessionRecord CreateRecord(
            string gameId,
            BciInputMode inputMode,
            float durationSec,
            int score,
            int correct,
            int wrong,
            float avgFocus,
            int pauseCount,
            string extraJson = null)
        {
            return new GameSessionRecord
            {
                RecordId = Guid.NewGuid().ToString("N"),
                TimestampUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                GameId = gameId,
                GameNameCn = GameIds.NameCn(gameId),
                InputMode = InputModeToString(inputMode),
                DurationSec = durationSec,
                Score = score,
                CorrectCount = correct,
                WrongCount = wrong,
                AvgFocus = avgFocus,
                PauseCount = pauseCount,
                ExtraJson = extraJson ?? ""
            };
        }

        public static string InputModeToString(BciInputMode mode) => mode switch
        {
            BciInputMode.Compensation => "compensation",
            BciInputMode.HybridBci => "hybrid_bci",
            BciInputMode.Mock => "mock",
            _ => "unknown"
        };

        private static GameSessionRecord ParseLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;
            var cols = SplitCsv(line);
            if (cols.Count < 11) return null;

            return new GameSessionRecord
            {
                RecordId = cols[0],
                TimestampUtc = cols[1],
                GameId = cols[2],
                GameNameCn = cols[3],
                InputMode = cols[4],
                DurationSec = float.TryParse(cols[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : 0f,
                Score = int.TryParse(cols[6], out var s) ? s : 0,
                CorrectCount = int.TryParse(cols[7], out var c) ? c : 0,
                WrongCount = int.TryParse(cols[8], out var w) ? w : 0,
                AvgFocus = float.TryParse(cols[9], NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : 0f,
                PauseCount = int.TryParse(cols[10], out var p) ? p : 0,
                ExtraJson = cols.Count > 11 ? cols[11] : ""
            };
        }

        private static List<string> SplitCsv(string line)
        {
            var result = new List<string>();
            var current = "";
            var inQuotes = false;
            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current += '"';
                        i++;
                    }
                    else inQuotes = !inQuotes;
                }
                else if (ch == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else current += ch;
            }
            result.Add(current);
            return result;
        }
    }
}

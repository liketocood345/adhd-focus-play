using System;

namespace ADHDTraining.Core.Session
{
    [Serializable]
    public class GameSessionRecord
    {
        public string RecordId;
        public string TimestampUtc;
        public string GameId;
        public string GameNameCn;
        public string InputMode;
        public float DurationSec;
        public int Score;
        public int CorrectCount;
        public int WrongCount;
        public float AvgFocus;
        public int PauseCount;
        public string ExtraJson;

        public static string CsvHeader =>
            "record_id,timestamp_utc,game_id,game_name_cn,input_mode,duration_sec,score,correct_count,wrong_count,avg_focus,pause_count,extra_json";

        public string ToCsvLine()
        {
            return string.Join(",",
                CsvEscape(RecordId),
                CsvEscape(TimestampUtc),
                CsvEscape(GameId),
                CsvEscape(GameNameCn),
                CsvEscape(InputMode),
                DurationSec.ToString("F2"),
                Score.ToString(),
                CorrectCount.ToString(),
                WrongCount.ToString(),
                AvgFocus.ToString("F1"),
                PauseCount.ToString(),
                CsvEscape(ExtraJson ?? ""));
        }

        private static string CsvEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}

namespace ADHDTraining.Core.Session
{
    public static class GameIds
    {
        public const string Selective = "selective";
        public const string Sustained = "sustained";
        public const string Shifting = "shifting";
        public const string Divided = "divided";
        public const string Inhibition = "inhibition";

        public static string NameCn(string gameId) => gameId switch
        {
            Selective => "听音寻宝",
            Sustained => "无尽跑酷者",
            Shifting => "指令反转",
            Divided => "双线救援",
            Inhibition => "红灯停绿灯行",
            _ => gameId
        };

        public static string SceneName(string gameId) => gameId switch
        {
            Selective => SceneNames.GameSelective,
            Sustained => SceneNames.GameSustained,
            Shifting => SceneNames.GameShifting,
            Divided => SceneNames.GameDivided,
            Inhibition => SceneNames.GameInhibition,
            _ => SceneNames.MainMenu
        };
    }
}

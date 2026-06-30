namespace ADHDTraining.Core.Session
{
    public interface IGameSessionReporter
    {
        string GameId { get; }
        int LiveScore { get; }
        int LiveCorrectCount { get; }
        int LiveWrongCount { get; }
    }
}

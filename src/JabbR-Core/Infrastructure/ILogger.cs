namespace JabbR_Core.Infrastructure
{
    public interface ILogger
    {
        void Log(LogType type, string message);
    }
}
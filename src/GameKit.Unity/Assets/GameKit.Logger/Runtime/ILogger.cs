namespace Microsoft.Extensions.Logging
{
#if !USE_ZLOGGER
    using UnityEngine;

    public interface ILogger
    {
    }

    public interface ILogger<out TCategoryName> : ILogger
    {
    }

    public static class LoggerExtensions
    {
        public static void LogDebug<T>(this ILogger<T> logger, string? message, params object?[] args)
        {
            if (logger is UnityLogger<T> unity)
            {
                unity.Log(message, args);
            }
        }
    }

    internal class UnityLogger<TCategoryName> : ILogger<TCategoryName>
    {
        public void Log(string message, params object[] args)
        {
            Debug.Log($"{typeof(TCategoryName).Name} | {string.Format(message, args)}");
        }
    }
#endif
}
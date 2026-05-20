using System;
using UnityEngine;

namespace CoreRacer.Services.Logging
{
    public static class LoggingExtensions
    {
        public static void DebugLog(this IGameLogger logger, LogCategory category, string message, UnityEngine.Object context = null)
        {
            logger?.Log(LogLevel.Debug, category, message, context);
        }

        public static void Info(this IGameLogger logger, LogCategory category, string message, UnityEngine.Object context = null)
        {
            logger?.Log(LogLevel.Info, category, message, context);
        }

        public static void Warn(this IGameLogger logger, LogCategory category, string message, UnityEngine.Object context = null)
        {
            logger?.Log(LogLevel.Warning, category, message, context);
        }

        public static void Error(this IGameLogger logger, LogCategory category, string message, UnityEngine.Object context = null)
        {
            logger?.Log(LogLevel.Error, category, message, context);
        }

        public static void Error(this IGameLogger logger, LogCategory category, Exception exception, UnityEngine.Object context = null)
        {
            logger?.Exception(category, exception, context);
        }
    }
}

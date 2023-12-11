using Microsoft.Extensions.Logging;

namespace ManzaTools.Utils;

internal static class LoggerExtension
{
    internal static string CreateLogMessage(this ILogger logger, string logMessage)
    {
        return $"[ManzaTools] - {logMessage ?? "N/A"}";
    }

    internal static void LogError(this ILogger logger, string logMessage, Exception ex)
    {
        logger.LogError(ex, $"[ManzaTools] - {logMessage ?? "N/A"}");
        //return $"[ManzaTools] - {logMessage ?? "N/A"}";
    }
}
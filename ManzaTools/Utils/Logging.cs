
namespace ManzaTools.Utils
{
    public static class Logging
    {
        public static void Log(string message)
        {
            Console.WriteLine("[ManzaTools] " + message);
        }

        internal static void Fatal(Exception ex, string module, string function)
        {
            Console.WriteLine($"[ManazTools] - FATAL {module}.{function} - {ex.Message} - Trace: {ex.StackTrace}{(ex.InnerException != null ? ($" - InnerMessage: {ex.Message} InnerTrace: {ex.InnerException.StackTrace}") : String.Empty)}");
        }
    }
}

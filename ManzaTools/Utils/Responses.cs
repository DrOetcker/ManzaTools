using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace ManzaTools.Utils
{
    public static class Responses
    {
        public static bool debugOutputsActive = false;

        public static void ReplyToPlayer(string message, CCSPlayerController player, bool isError = false, bool toConsole = false)
        {
            if (toConsole)
                player.PrintToConsole($"{(isError ? Statics.ConsolePrefixError : Statics.ConsolePrefix)} {message}");
            else
                player.PrintToChat(Statics.GetChatText(message, isError));
        }

        public static void ReplyToServer(string message, bool isError = false, bool toConsole = false)
        {
            if (toConsole)
                Server.PrintToConsole($"{(isError ? Statics.ConsolePrefixError : Statics.ConsolePrefix)} {message}");
            else
                Server.PrintToChatAll(Statics.GetChatText(message, isError));
        }

        public static void SendDebug(string message, string? module, string? function)
        {
            if (!debugOutputsActive)
                return;
            if(!string.IsNullOrEmpty(module) && !string.IsNullOrEmpty(function))
            {
                Server.PrintToChatAll(Statics.GetDebugChatText($"{module}.{function}: {message}"));
                return;
            }
            if (!string.IsNullOrEmpty(module))
            {
                Server.PrintToChatAll(Statics.GetDebugChatText($"{module}: {message}"));
                return;
            }
            if (!string.IsNullOrEmpty(function))
            {
                Server.PrintToChatAll(Statics.GetDebugChatText($"{function}: {message}"));
                return;
            }
            Server.PrintToChatAll(Statics.GetDebugChatText(message));
        }
    }
}

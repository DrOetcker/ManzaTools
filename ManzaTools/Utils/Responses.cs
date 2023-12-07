using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;

namespace ManzaTools.Utils
{
    public static class Responses
    {
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

    }
}

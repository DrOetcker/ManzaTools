using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using ManzaTools.Models;

namespace ManzaTools.Utils
{
    static class Statics
    {
        public static string CfgPath = Path.Join(Server.GameDirectory, "csgo", "cfg", "ManzaTools");
        public static string ConsolePrefix = "[ManzaTools]";
        public static string ConsolePrefixError = "[ManzaTools - Error]";
        private static string ChatPrefix = $"[{ChatColors.Green}ManzaTools{ChatColors.Default}]";
        private static string ChatErrorPrefix = $"[{ChatColors.Red}ManzaTools{ChatColors.Default}]";
        public static string GetChatText(string textToWrite, bool isError = false)
        {
            if (isError)
                return $"{ChatErrorPrefix} {textToWrite}";
            return $"{ChatPrefix} {textToWrite}";
        }
        public static Dictionary<GameModeEnum, string> GameModeCfgs = new Dictionary<GameModeEnum, string>()
        {
            {GameModeEnum.Practice, "practice.cfg" },
            {GameModeEnum.PracticeMatch, "practiceMatch.cfg" },
            {GameModeEnum.Deathmatch, "deathMatch.cfg" }
        };
    }
}
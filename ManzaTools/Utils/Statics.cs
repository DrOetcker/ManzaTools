using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;

using ManzaTools.Models;

namespace ManzaTools.Utils
{
    static class Statics
    {
        public static readonly string CfgPath = Path.Join(Server.GameDirectory, "csgo", "cfg", "ManzaTools");
        public static readonly string ConsolePrefix = "[ManzaTools]";
        public static readonly string ConsolePrefixError = "[ManzaTools - Error]";
        private static readonly string ChatPrefix = $"[{ChatColors.Green}ManzaTools{ChatColors.Default}]";
        private static readonly string DebugPrefix = $"[{ChatColors.Yellow}ManzaTools Debug{ChatColors.Default}]";
        private static readonly string ChatErrorPrefix = $"[{ChatColors.Red}ManzaTools{ChatColors.Default}]";
        public static readonly Dictionary<GameModeEnum, string> GameModeCfgs = new()
        {
            {GameModeEnum.Practice, "practice.cfg" },
            {GameModeEnum.PracticeMatch, "practiceMatch.cfg" },
            {GameModeEnum.Deathmatch, "deathMatch.cfg" }
        };

        public static string GetChatText(string textToWrite, bool isError = false)
        {
            if (isError)
                return $"{ChatErrorPrefix} {textToWrite}";
            return $"{ChatPrefix} {textToWrite}";
        }

        public static string GetDebugChatText(string textToWrite)
        {
            return $"{DebugPrefix} {textToWrite}";
        }
    }
}
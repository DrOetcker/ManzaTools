using CounterStrikeSharp.API.Core;

namespace ManzaTools.Extensions
{
    public static class PlayerExtension
    {
        private static Dictionary<byte, string> teamEnumMap = new Dictionary<byte, string>() {
            {2, "t" },
            {3, "ct" }
        };
        public static bool IsCounterTerrorist(this CCSPlayerController player)
        {
            return player.TeamNum == 3;
        }

        public static bool IsCounterTerrorist(byte teamNum)
        {
            return teamNum == 3;
        }

        public static string TeamNumToTeam(byte teamNum)
        {
            return teamEnumMap.FirstOrDefault(x => x.Key == teamNum).Value;
        }

        public static byte TeamToTeamNum(string team)
        {
            return teamEnumMap.FirstOrDefault(x => x.Value == team).Key;
        }
    }
}

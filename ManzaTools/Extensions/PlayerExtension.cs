using CounterStrikeSharp.API.Core;

namespace ManzaTools.Extensions
{
    public static class PlayerExtension
    {
        //TeamNum 3 = CT
        //TeamNum 2 = T
        public static bool IsCounterTerrorist(this CCSPlayerController player) {
            return player.TeamNum == 3; 
        }

        public static bool IsCounterTerrorist(byte teamNum)
        {
            return teamNum == 3;
        }
    }
}

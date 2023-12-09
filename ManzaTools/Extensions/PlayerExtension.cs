using CounterStrikeSharp.API.Core;

namespace ManzaTools.Extensions
{
    public static class PlayerExtension
    {
        public static bool IsCounterTerrorist(this CCSPlayerController player) {
            return player.TeamNum == 3; 
        }
    }
}

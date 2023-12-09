using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace ManzaTools.Models
{
    internal class PlacedBots
    {
        public Vector Position { get; set; }
        public QAngle Angle { get; set; }
        public bool Crouch { get; set; }
        public string PlayerName { get; set; }
        public CCSPlayerController Owner { get; set; }
        public CCSPlayerController Bot { get; set; }
    }
}
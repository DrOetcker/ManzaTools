using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace ManzaTools.Models
{
    internal class PlacedBots
    {
        public QAngle Angle { get; set; }

        public CCSPlayerController Bot { get; set; }

        public bool Crouch { get; set; }

        public CCSPlayerController Owner { get; set; }

        public string PlayerName { get; set; }

        public Vector Position { get; set; }
    }
}
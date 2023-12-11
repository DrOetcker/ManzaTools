using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace ManzaTools.Models
{
    internal class PlacedBots
    {
        public QAngle Angle { get; set; } = null!;

        public CCSPlayerController Bot { get; set; } = null!;

        public bool Crouch { get; set; }

        public CCSPlayerController Owner { get; set; } = null!;

        public string? PlayerName { get; set; }

        public Vector Position { get; set; } = null!;
    }
}
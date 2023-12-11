
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace ManzaTools.Models
{
    internal class PersonalThrownGrenade
    {
        public CCSPlayerController User { get; internal set; }
        public string Weapon { get; internal set; }
        public QAngle Angle { get; set; } = null!;
        public Vector Position { get; set; } = null!;
    }
}
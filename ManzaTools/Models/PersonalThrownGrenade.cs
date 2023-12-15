
using CounterStrikeSharp.API.Modules.Utils;

namespace ManzaTools.Models
{
    internal class PersonalThrownGrenade
    {
        public ulong UserSteamId { get; internal set; }
        public string Weapon { get; internal set; }
        public QAngle Angle { get; set; } = null!;
        public Vector Position { get; set; } = null!;
    }
}
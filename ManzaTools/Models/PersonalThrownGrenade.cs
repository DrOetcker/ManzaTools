
using CounterStrikeSharp.API.Modules.Utils;

namespace ManzaTools.Models
{
    internal class PersonalThrownGrenade
    {
        public ulong UserSteamId { get; internal set; }
        public string Weapon { get; internal set; }
        public QAngle PlayerAngle { get; set; } = null!;
        public Vector PlayerPosition { get; set; } = null!;
        public string Position { get; internal set; }
        public string Velocity { get; internal set; }
    }
}
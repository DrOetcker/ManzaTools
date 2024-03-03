using CounterStrikeSharp.API.Modules.Utils;

namespace ManzaTools.Models
{
    public class RecordedNade
    {
        public QAngle Angle { get; set; } = null!;
        public Vector Position { get; set; } = null!;
        public Vector Velocity { get; set; } = null!;
        public TimeSpan ThrownAt { get; set; }
        public string Type { get; internal set; }
    }
}
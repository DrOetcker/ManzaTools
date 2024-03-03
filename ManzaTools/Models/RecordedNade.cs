using CounterStrikeSharp.API.Modules.Utils;

namespace ManzaTools.Models
{
    public class RecordedNade
    {
        public string Angle { get; set; } = null!;
        public string Position { get; set; } = null!;
        public string Velocity { get; set; } = null!;
        public TimeSpan ThrownAt { get; set; }
        public string Type { get; internal set; }
    }
}
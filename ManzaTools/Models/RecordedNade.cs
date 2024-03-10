namespace ManzaTools.Models
{
    public class RecordedNade
    {
        public uint Id { get; set; }
        public string Position { get; set; } = null!;
        public string Velocity { get; set; } = null!;
        public TimeSpan ThrownAt { get; set; }
        public string Type { get; set; } = null!;
        public string PlayerPosition { get; set; }
        public string PlayerAngle { get; set; }
    }
}
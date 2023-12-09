namespace ManzaTools.Models
{
    internal class SavedNade
    {
        public uint Id { get; set; }
        public string Map { get; set; }
        public string Type { get; set; }
        public string Team { get; set; }
        public string Description { get; set; }
        public string PlayerPosition { get; set; }
        public string PlayerAngle { get; set; }
        public string Name { get; set; }
        public string CreatorSteamId { get; set; }
        public string Creator { get; set; }
        public bool IsImported { get; set; } = false;
    }
}

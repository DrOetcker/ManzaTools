namespace ManzaTools.Models
{
    internal class SavedNade
    {
        public string Creator { get; set; } = null!;

        public string CreatorSteamId { get; set; } = null!;

        public string? Description { get; set; }

        public uint Id { get; set; }

        public bool IsImported { get; set; } = false;

        public string Map { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string PlayerAngle { get; set; } = null!;

        public string PlayerPosition { get; set; } = null!;

        public string? Team { get; set; }

        public string Type { get; set; } = null!;
    }
}

namespace ManzaTools.Models
{
    internal class SavedReplay
    {
        public string Creator { get; set; }
        public string CreatorSteamId { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public uint Id { get; set; }
        public string Map { get; set; } = null!;
        public string Name { get; set; }
        public IList<RecordedNade> RecordedNades { get; set; }
        public DateTime RecordingTime { get; set; }
    }
}
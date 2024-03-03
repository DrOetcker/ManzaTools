namespace ManzaTools.Models
{
    internal class Replay
    {
        public Replay(ulong steamID, DateTime startTime)
        {
            SteamId = steamID;
            RecordingTime = startTime;
            RecordedNades = new List<RecordedNade>();
        }

        public ulong SteamId { get; }
        public DateTime RecordingTime { get; }
        public uint Id { get; set; }
        public bool Finished { get; set; }
        public IList<RecordedNade> RecordedNades { get; }
        public TimeSpan Duration { get; internal set; }
        public string Map { get; set; } = null!;
        public string Creator { get; internal set; }
        public string CreatorSteamId { get; internal set; }
        public object Name { get; internal set; }
        public object Description { get; internal set; }
    }
}
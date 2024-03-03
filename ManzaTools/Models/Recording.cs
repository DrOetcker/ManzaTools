namespace ManzaTools.Models
{
    internal class Recording
    {
        public Recording(ulong steamID, int recordingIndex, DateTime startTime)
        {
            SteamId = steamID;
            RecordingIndex = recordingIndex;
            RecordingTime = startTime;
            RecordedNades = new List<RecordedNade>();
        }

        public ulong SteamId { get; }
        public int RecordingIndex { get; }
        public DateTime RecordingTime { get; }
        public bool Finished { get; set; }
        public IList<RecordedNade> RecordedNades { get; }
        public TimeSpan Duration { get; internal set; }
    }
}
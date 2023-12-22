using CounterStrikeSharp.API.Core;

namespace ManzaTools.Models
{
    internal class Recording
    {
        public Recording(ulong steamID, int recordingIndex, DateTime startTime)
        {
            SteamId = steamID;
            RecordingIndex = recordingIndex;
            StartTime = startTime;
            NadeEvents = new List<EventGrenadeThrown>();
        }

        public ulong SteamId { get; }
        public int RecordingIndex { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; set; }
        public bool Finished { get; set; }
        public virtual TimeSpan Duration => EndTime == null || StartTime == null ? new TimeSpan() : EndTime - StartTime;

        public IList<EventGrenadeThrown> NadeEvents { get; }
    }
}
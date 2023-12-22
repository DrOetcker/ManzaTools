using CounterStrikeSharp.API.Core;

namespace ManzaTools.Interfaces
{
    public interface IRecordService : IPracticeBaseService
    {
        HookResult OnGrenadeThrown(EventGrenadeThrown @event, GameEventInfo info);
    }
}

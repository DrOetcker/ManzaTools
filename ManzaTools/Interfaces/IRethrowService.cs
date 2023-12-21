using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces;

public interface IRethrowService : IPracticeBaseService
{
    HookResult OnGrenadeThrown(EventGrenadeThrown @event, GameEventInfo info);
    void Rethrow(CCSPlayerController? player, CommandInfo info);
    void Last(CCSPlayerController? player, CommandInfo info);
}

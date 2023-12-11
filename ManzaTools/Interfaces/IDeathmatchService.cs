using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces;

public interface IDeathmatchService
{
    HookResult HandlePlayerDeath(EventPlayerDeath @event, GameEventInfo info);

    void StartDeathmatch(CCSPlayerController? player, CommandInfo info);
}

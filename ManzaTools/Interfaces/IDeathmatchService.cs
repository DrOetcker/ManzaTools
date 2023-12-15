using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces;

public interface IDeathmatchService
{
    HookResult GetRandomizedWeapon(EventPlayerSpawn @event, GameEventInfo info);
    HookResult HandlePlayerDeath(EventPlayerDeath @event, GameEventInfo info);

    void StartDeathmatch(CCSPlayerController? player, CommandInfo info);
}

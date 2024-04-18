using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces;

public interface IBotService: IPracticeBaseService
{
    void CreateBot(CCSPlayerController? player, CommandInfo info);

    HookResult PositionBotOnRespawn(EventPlayerSpawn @event, GameEventInfo info);

    void RemoveBot(CCSPlayerController? player, CommandInfo info);

    void RemoveBots(CCSPlayerController? player, CommandInfo info);
}

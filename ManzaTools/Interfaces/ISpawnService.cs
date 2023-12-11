using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces;

public interface ISpawnService
{
    void SetPlayerPosition(CCSPlayerController? player, CommandInfo info);
}

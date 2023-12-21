using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces;

public interface ISpawnService : IPracticeBaseService
{
    void SetPlayerPosition(CCSPlayerController? player, CommandInfo info);
}

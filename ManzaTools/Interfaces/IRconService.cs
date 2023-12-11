using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces;

public interface IRconService
{
    void Execute(CCSPlayerController? player, CommandInfo info);
}

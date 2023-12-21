using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces;

public interface IRconService: IBaseService
{
    void Execute(CCSPlayerController? player, CommandInfo info);
}

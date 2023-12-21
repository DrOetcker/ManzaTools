using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces;

public interface IChangeMapService: IBaseService
{
    void Changemap(CCSPlayerController? player, CommandInfo command);

    void PreLoadAvailableMaps();
}

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

using ManzaTools.Models;

namespace ManzaTools.Interfaces;

public interface IChangeMapService: IBaseService
{
    void Changemap(CCSPlayerController? player, CommandInfo command, IList<Map> availableMaps);

    IList<Map> PreLoadAvailableMaps();
}

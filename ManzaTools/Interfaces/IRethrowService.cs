using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces;

public interface IRethrowService
{
    void Rethrow(CCSPlayerController? player, CommandInfo info);
}

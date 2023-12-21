using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces;

public interface IClearService : IPracticeBaseService
{
    void ClearUtilities(CCSPlayerController? player, CommandInfo info);
}

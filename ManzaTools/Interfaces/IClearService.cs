using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces;

public interface IClearService
{
    void ClearUtilities(CCSPlayerController? player, CommandInfo info);

    void AddCommands(Action<string, string, CommandInfo.CommandCallback> addCommand);
}

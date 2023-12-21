using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces;

public interface IEndRoundService : IPracticeBaseService
{
    void EndRound(CCSPlayerController? player, CommandInfo info);
}

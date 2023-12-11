using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

using ManzaTools.Interfaces;
using ManzaTools.Utils;

using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class EndRoundService : PracticeBaseService, IEndRoundService
    {
        protected EndRoundService(ILogger<EndRoundService> logger, IGameModeService gameModeService)
            : base(logger, gameModeService)
        {
        }

        public void EndRound(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPracticeMatch)
            {
                Responses.ReplyToPlayer("Could not end round - Not in PracticeMatch", player, true);
                return;
            }
            Server.ExecuteCommand("endround");
            Responses.ReplyToServer("Start new round - GLHF");
        }
    }
}

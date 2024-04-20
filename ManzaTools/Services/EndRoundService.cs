using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using ManzaTools.Extensions;
using ManzaTools.Interfaces;
using ManzaTools.Utils;

using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class EndRoundService : PracticeBaseService, IEndRoundService
    {
        public EndRoundService(ILogger<EndRoundService> logger, IGameModeService gameModeService)
            : base(logger, gameModeService)
        {
        }

        public override void Init(ManzaTools manzaTools)
        {
            manzaTools.AddCommand("css_endround", "Ends a round in a PractiveMatch", EndRound);
        }

        public void EndRound(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPracticeMatch)
            {
                if (player != null)
                    Responses.ReplyToPlayer("Could not end round - Not in PracticeMatch", player, true);
                return;
            }

            if(_gameModeService.CurrentGameMode == Models.GameModeEnum.PracticeMatch)
            {
                if (PlayerExtension.IsCounterTerrorist(player!.TeamNum))
                    Server.ExecuteCommand("bot_add_t");
                else
                    Server.ExecuteCommand("bot_add_ct");

                Server.ExecuteCommand("bot_kick all");
            }
            else
            {
                Server.ExecuteCommand("bot_kill all");
            }




            Responses.ReplyToServer("Start new round - GLHF");
        }
    }
}

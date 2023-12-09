﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using ManzaTools.Utils;

namespace ManzaTools.Services
{
    public class EndRoundService : PracticeBaseService
    {
        public EndRoundService(GameModeService gameModeService)
            : base(gameModeService)
        {
        }

        internal void EndRound(CCSPlayerController? player, CommandInfo info)
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

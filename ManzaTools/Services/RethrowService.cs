using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

using ManzaTools.Interfaces;

using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class RethrowService : PracticeBaseService, IRethrowService
    {
        protected RethrowService(ILogger<RethrowService> logger, IGameModeService gameModeService)
            : base(logger, gameModeService)
        {
        }

        public void Rethrow(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice)
                return;
            Server.ExecuteCommand("sv_rethrow_last_grenade");
        }
    }
}

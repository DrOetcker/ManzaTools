using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Services
{
    public class RethrowService
    {
        private readonly GameModeService _gameModeService;

        public RethrowService(GameModeService gameModeService)
        {
            _gameModeService = gameModeService;
        }
        internal void Rethrow(CCSPlayerController? player, CommandInfo info)
        {
            if (!_gameModeService.IsPractice())
                return;
            Server.ExecuteCommand("sv_rethrow_last_grenade");
        }
    }
}

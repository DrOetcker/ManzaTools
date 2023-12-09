using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Services
{
    public class RethrowService : PracticeBaseService
    {
        public RethrowService(GameModeService gameModeService)
            : base(gameModeService)
        {
        }

        internal void Rethrow(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice)
                return;
            Server.ExecuteCommand("sv_rethrow_last_grenade");
        }
    }
}

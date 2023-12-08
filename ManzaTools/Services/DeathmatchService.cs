using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using ManzaTools.Models;

namespace ManzaTools.Services
{
    public class DeathmatchService
    {
        private readonly GameModeService _gameModeService;

        public DeathmatchService(GameModeService gameModeService)
        {
            _gameModeService = gameModeService;
        }

        internal void StartDeathmatch(CCSPlayerController? player, CommandInfo info)
        {
            _gameModeService.LoadGameMode(GameModeEnum.Deathmatch);
            if (info?.ArgCount > 1)
            {
                if (info.ArgString.ToLower().Contains("nobots"))
                    Server.ExecuteCommand("exec nobots.cfg");
                if (info.ArgString.ToLower().Contains("hsonly"))
                    Server.ExecuteCommand("exec hsonly.cfg");
                if (info.ArgString.ToLower().Contains("pistol"))
                    Server.ExecuteCommand("exec pistolsonly.cfg");
                if (info.ArgString.ToLower().Contains("team"))
                    Server.ExecuteCommand("exec teamdm.cfg");
            }
        }
    }
}

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using ManzaTools.Models;
using Microsoft.Extensions.Logging;

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
                    Server.ExecuteCommand($"execifexists {Path.Combine("ManzaTools", "dmNoBots.cfg")}");
                if (info.ArgString.ToLower().Contains("hsonly"))
                    Server.ExecuteCommand($"execifexists {Path.Combine("ManzaTools", "dmHsOnly.cfg")}");
                if (info.ArgString.ToLower().Contains("pistol"))
                    Server.ExecuteCommand($"execifexists {Path.Combine("ManzaTools", "dmPistolsOnly.cfg")}");
                if (info.ArgString.ToLower().Contains("team"))
                    Server.ExecuteCommand($"execifexists {Path.Combine("ManzaTools", "dmTeamDm.cfg")}");
            }
        }

        public HookResult HandlePlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            if (_gameModeService.CurrentGameMode != GameModeEnum.Deathmatch)
                return HookResult.Continue;

            var activeWeapon = @event.Attacker.PlayerPawn.Value.WeaponServices.ActiveWeapon;
            if (activeWeapon != null)
            {
                activeWeapon.Value.Clip1 = 250;
                activeWeapon.Value.ReserveAmmo[0] = 250;
            }

            return HookResult.Continue;
        }
    }
}

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

            var attacker = @event.Attacker;
            var activeWeapon = attacker.PlayerPawn.Value.WeaponServices.ActiveWeapon;
            if (activeWeapon != null)
            {
                activeWeapon.Value.Clip1 = 250;
                activeWeapon.Value.ReserveAmmo[0] = 250;
            }
            //untested
            //Server.PrintToChatAll($"Incl health");
            //@event.Attacker.PlayerPawn.Value.Health += 20;
            //@event.Attacker.Health += 20;
            //info.DontBroadcast = false;

            //NativeAPI.FireEventToClient("ammo_refill", @event.Attacker.Index);
            //@event.FireEventToClient(@event.Attacker.OriginalControllerOfCurrentPawn.Value);
            //@event.Attacker.OriginalControllerOfCurrentPawn.Value.Fire
            //@event.Attacker.Pawn
            //@event.Attacker.PlayerPawn.Value.event
            Server.PrintToChatAll($"{@event.Attacker.PlayerName} killed {@event.Userid.PlayerName}");
            return HookResult.Continue;
        }
    }
}

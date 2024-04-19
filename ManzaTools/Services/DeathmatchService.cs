using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using ManzaTools.Interfaces;
using ManzaTools.Models;
using ManzaTools.Utils;
using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class DeathmatchService : BaseService, IDeathmatchService
    {
        private readonly IGameModeService _gameModeService;
        private bool _isPistolMode;

        public DeathmatchService(ILogger<DeathmatchService> logger, IGameModeService gameModeService)
        : base(logger)
        {
            _gameModeService = gameModeService;
        }

        public override void Init(ManzaTools manzaTools)
        {
            manzaTools.AddCommand("css_deathmatch", "Changes the current GameMode to deathmatch", StartDeathmatch);
            manzaTools.RegisterEventHandler<EventPlayerSpawn>(GetRandomizedWeapon);
            manzaTools.RegisterEventHandler<EventPlayerDeath>(HandlePlayerDeath);
        }

        public HookResult GetRandomizedWeapon(EventPlayerSpawn @event, GameEventInfo info)
        {
            if (_gameModeService.CurrentGameMode != GameModeEnum.Deathmatch)
                return HookResult.Continue;


            var bot = @event.Userid;
            if (bot.IsValid && bot.IsBot && bot.UserId.HasValue && !bot.IsHLTV)
            {
                var random = new Random(Guid.NewGuid().GetHashCode());
                var pistolEnum = random.Next(200, 210);
                CsItem nextWeapon;

                if (_isPistolMode)
                    nextWeapon = (CsItem)pistolEnum;
                else
                {
                    var midTierEnum = random.Next(300, 308);
                    var riflesTierEnum = random.Next(400, 411);
                    var nextWeaponSource = random.Next(0, 2);
                    switch (nextWeaponSource)
                    {
                        case 0:
                            nextWeapon = (CsItem)midTierEnum;
                            break;
                        case 1:
                            nextWeapon = (CsItem)riflesTierEnum;
                            break;
                        default:
                            nextWeapon = CsItem.AK47;
                            break;
                    }
                }
                bot.GiveNamedItem(nextWeapon);
            }
            return HookResult.Continue;
        }

        public HookResult HandlePlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            if (_gameModeService.CurrentGameMode != GameModeEnum.Deathmatch || @event.Attacker?.PlayerPawn.Value?.WeaponServices == null)
                return HookResult.Continue;

            var activeWeapon = @event.Attacker.PlayerPawn.Value.WeaponServices.ActiveWeapon;
            if (activeWeapon.Value != null)
            {
                //activeWeapon.Value.Clip1 = 250;
                activeWeapon.Value.ReserveAmmo[0] = 250;
            }

            return HookResult.Continue;
        }

        public void StartDeathmatch(CCSPlayerController? player, CommandInfo info)
        {
            _gameModeService.LoadGameMode(GameModeEnum.Deathmatch);
            _isPistolMode = false;
            if (info.ArgCount <= 1)
                return;

            var startupInfo = new List<string>();
            if (info.ArgString.ToLower().Contains("nobots"))
            {
                Server.ExecuteCommand($"execifexists {Path.Combine("ManzaTools", "dmNoBots.cfg")}");
                startupInfo.Add("NoBots");
            }
            if (info.ArgString.ToLower().Contains("hsonly"))
            {
                Server.ExecuteCommand($"execifexists {Path.Combine("ManzaTools", "dmHsOnly.cfg")}");
                startupInfo.Add("Headshot only");
            }
            if (info.ArgString.ToLower().Contains("pistol"))
            {
                _isPistolMode = true;
                Server.ExecuteCommand($"execifexists {Path.Combine("ManzaTools", "dmPistolsOnly.cfg")}");
                startupInfo.Add("Pistols only");
            }
            if (info.ArgString.ToLower().Contains("team"))
            {
                Server.ExecuteCommand($"execifexists {Path.Combine("ManzaTools", "dmTeamDm.cfg")}");
                startupInfo.Add("TeamDeathmatch");
            }
            if (startupInfo.Count > 0)
                Responses.ReplyToServer($"DMMode : {string.Join(", ", startupInfo)}");
        }
    }
}

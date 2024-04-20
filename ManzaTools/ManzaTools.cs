using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using ManzaTools.Config;
using ManzaTools.Interfaces;
using ManzaTools.Models;
using ManzaTools.Services;
using ManzaTools.Utils;

using Microsoft.Extensions.Logging;

namespace ManzaTools
{
    public class ManzaTools : BasePlugin, IPluginConfig<ManzaToolsConfig>
    {
        private readonly IBotService _botService;
        private readonly ICfgShipperService _cfgShipper;
        private readonly IChangeMapService _changeMapService;
        private readonly IClearService _clearService;
        private readonly IDeathmatchService _deathmatchService;
        private readonly IEffectService _effectService;
        private readonly IEndRoundService _endRoundService;
        private readonly IGameModeService _gameModeService;
        private readonly IRconService _rconService;
        private readonly IRethrowService _rethrowService;
        private readonly ISavedNadesService _savedNadesService;
        private readonly ISpawnService _spawnService;
        private readonly IRecordService _recordService;
        private readonly IAdminService _adminService;

        public ManzaTools(
            ICfgShipperService cfgShipper,
            IGameModeService gameModeService,
            IEffectService effectService,
            IChangeMapService changeMapService,
            IDeathmatchService deathmatchService,
            ISpawnService spawnService,
            IClearService clearService,
            IRconService rconService,
            IRethrowService rethrowService,
            IEndRoundService endRoundService,
            ISavedNadesService savedNadesService,
            IBotService botService,
            IRecordService recordService,
            IAdminService adminService)
        {
            _cfgShipper = cfgShipper;
            _gameModeService = gameModeService;
            _effectService = effectService;
            _changeMapService = changeMapService;
            _deathmatchService = deathmatchService;
            _spawnService = spawnService;
            _clearService = clearService;
            _rconService = rconService;
            _rethrowService = rethrowService;
            _endRoundService = endRoundService;
            _savedNadesService = savedNadesService;
            _botService = botService;
            _recordService = recordService;
            _adminService = adminService;
        }

        public override string ModuleAuthor => "DrOetcker";

        public override string ModuleName => "ManzaTools";

        public override string ModuleVersion => "1.0.0";

        public ManzaToolsConfig Config { get; set; } = new();

        public void OnConfigParsed(ManzaToolsConfig config)
        {
            try
            {
                Config = config;
                //_effectService.SetSmokeTimerEnabled(Config.SmokeTimerEnabled);
                //_effectService.SetBlindTimerEnabled(Config.BlindTimerEnabled);
                //_effectService.SetDamageReportEnabled(Config.DamageReportEnabled);
                _changeMapService.PreLoadAvailableMaps();
                if (Config.DefaultGameMode != GameModeEnum.Disabled)
                {
                    InitServices();
                    InitTestPlugin();
                    InitDebugOutput();
                }
                RegisterListener((Listeners.OnMapStart)(entity =>
                {
                    SavedNadesService.VerifySavedNadesFileExists();
                    RecordService.VerifySavedReplaysFileExists();
                }));
            }
            catch (Exception ex)
            {
                Logger.LogError("Could not parse config", ex);
            }

            Responses.ReplyToServer("ConfigParsed ManzaTools", false, true);
        }

        public override void Load(bool hotReload)
        {
            _cfgShipper.InitDefaultCfgs(ModuleDirectory);
            Responses.ReplyToServer("Loaded ManzaTools", false, true);
            //RegisterEventHandler<EventRoundStart>((@event, info) =>
            //{
            //    Logger.LogInformation("Round has started with time limit of {Timelimit}", @event.Timelimit);

            //    return HookResult.Continue;
            //}); -- uncomment to kill server
        }

        public override void Unload(bool hotReload)
        {
            Responses.ReplyToServer("Unloaded ManzaTools", false, true);
        }

        private void InitServices()
        {
            //_botService.Init(this); - Kills server
            _changeMapService.Init(this);
            _clearService.Init(this);
            //_deathmatchService.Init(this); - Kills server
            //_effectService.Init(this); - Kills server
            _endRoundService.Init(this);
            _gameModeService.Init(this);
            _rconService.Init(this);
            //_rethrowService.Init(this); - Kills server
            _savedNadesService.Init(this);
            _spawnService.Init(this);
            //_recordService.Init(this); - Kills server
            _adminService.Init(this);
        }

        private void InitTestPlugin()
        {
            AddCommand("css_testplugin", "Tests if the plugin is running", PerformPluginTest);
        }

        private void InitDebugOutput()
        {
            AddCommand("css_debug", "Tests if the plugin is running", ToggleDebug);
        }

        private void ToggleDebug(CCSPlayerController? player, CommandInfo commandInfo)
        {
            Responses.debugOutputsActive = !Responses.debugOutputsActive;
            Responses.ReplyToServer($"Debug outputs active: {Responses.debugOutputsActive}");
        }

        private void PerformPluginTest(CCSPlayerController? player, CommandInfo info)
        {
            if (player == null)
                Responses.ReplyToServer("ManzaTools läuft");
            else
                Responses.ReplyToPlayer("ManzaTools läuft", player);
            Responses.ReplyToServer($"ArgCount = {info.ArgCount}");
        }
    }
}

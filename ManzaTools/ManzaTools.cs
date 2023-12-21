using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

using ManzaTools.Config;
using ManzaTools.Interfaces;
using ManzaTools.Models;
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
            IRecordService recordService)
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
                _effectService.SetSmokeTimerEnabled(Config.SmokeTimerEnabled);
                _effectService.SetBlindTimerEnabled(Config.BlindTimerEnabled);
                _effectService.SetDamageReportEnabled(Config.DamageReportEnabled);
                if (Config.DefaultGameMode != GameModeEnum.Disabled)
                    RegisterListeners();
            }
            catch (Exception ex)
            {
                Logger.LogError("Could not parse config", ex);
            }

            Responses.ReplyToServer("ConfigParsed ManzaTools", false, true);
        }

        private void InitBots()
        {
            this._botService.AddCommands(AddCommand);
            RegisterEventHandler<EventPlayerSpawn>((@event, info) => _botService.PositionBotOnRespawn(@event, info));
        }

        private void InitChangeMap()
        {
            this._changeMapService.PreLoadAvailableMaps();
            this._changeMapService.AddCommands(AddCommand);
            RegisterListener((Listeners.OnMapStart)(entity => _gameModeService.LoadGameMode(GameModeEnum.Practice)));
        }

        private void InitClear()
        {
            this._clearService.AddCommands(AddCommand);
        }

        private void InitDeathMatch()
        {
            this._deathmatchService.AddCommands(AddCommand);
            RegisterEventHandler<EventPlayerSpawn>((@event, info) => _deathmatchService.GetRandomizedWeapon(@event, info));
            RegisterEventHandler<EventPlayerDeath>((@event, info) => _deathmatchService.HandlePlayerDeath(@event, info));
        }

        private void InitEffectTimers()
        {
            this._effectService.AddCommands(AddCommand);
            RegisterListener((Listeners.OnEntitySpawned)(entity => _effectService.OnEntitySpawn(entity)));
            RegisterEventHandler<EventSmokegrenadeDetonate>((@event, info) => _effectService.OnSmokeGrenadeDetonate(@event, info));
            RegisterEventHandler<EventPlayerBlind>((@event, info) => _effectService.OnPlayerBlind(@event, info));
            RegisterEventHandler<EventPlayerHurt>((@event, info) => _effectService.OnPlayerDamage(@event, info));
        }

        private void InitEndRound()
        {
            this._endRoundService.AddCommands(AddCommand);
        }

        private void InitGameModes()
        {
            this._gameModeService.AddCommands(AddCommand);
        }

        private void InitRcon()
        {
            this._rconService.AddCommands(AddCommand);
        }

        private void InitRethrow()
        {
            this._rethrowService.AddCommands(AddCommand);
            RegisterEventHandler<EventGrenadeThrown>((@event, info) => _rethrowService.OnGrenadeThrown(@event, info));
        }

        private void InitSavedNades()
        {
            this._savedNadesService.AddCommands(AddCommand);
        }

        private void InitSpawn()
        {
            this._spawnService.AddCommands(AddCommand);
        }

        private void InitTestPlugin()
        {
            AddCommand("css_testplugin", "Tests if the plugin is running", (player, info) => PerformPluginTest(player, info));
        }

        private void PerformPluginTest(CCSPlayerController? player, CommandInfo info)
        {
            if (player == null)
                Responses.ReplyToServer("ManzaTools läuft");
            else
                Responses.ReplyToPlayer("ManzaTools läuft", player);
            Responses.ReplyToServer($"ArgCount = {info.ArgCount}");
            Responses.ReplyToServer($"Arg 1 = {info.ArgByIndex(1)}");
            Responses.ReplyToServer($"Arg 2 = {info.ArgByIndex(2)}");
        }

        public override void Load(bool hotReload)
        {
            _cfgShipper.InitDefaultCfgs(ModuleDirectory);
            Responses.ReplyToServer("Loaded ManzaTools", false, true);
        }

        private void RegisterListeners()
        {
            InitTestPlugin();
            InitEffectTimers();
            InitChangeMap();
            InitGameModes();
            InitDeathMatch();
            InitSpawn();
            InitClear();
            InitRcon();
            InitRethrow();
            InitEndRound();
            InitSavedNades();
            InitBots();
        }

        public override void Unload(bool hotReload)
        {
            Responses.ReplyToServer("Unloaded ManzaTools", false, true);
        }
    }
}

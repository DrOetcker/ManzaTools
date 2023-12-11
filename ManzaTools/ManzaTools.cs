using CounterStrikeSharp.API.Core;

using ManzaTools.Config;
using ManzaTools.Interfaces;
using ManzaTools.Models;
using ManzaTools.Utils;

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
            IBotService botService)
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
        }

        public override string ModuleAuthor => "DrOetcker";

        public override string ModuleName => "ManzaTools";

        public override string ModuleVersion => "0.1.0";

        public ManzaToolsConfig Config { get; set; } = new();

        public void OnConfigParsed(ManzaToolsConfig config)
        {
            try
            {
                Config = config;
                Config.AvailableMaps = _changeMapService.PreLoadAvailableMaps();
                this._effectService.SetSmokeTimerEnabled(Config.SmokeTimerEnabled);
                this._effectService.SetBlindTimerEnabled(Config.BlindTimerEnabled);
                this._effectService.SetDamageReportEnabled(Config.DamageReportEnabled);
                if (Config.DefaultGameMode != GameModeEnum.Disabled)
                    RegisterListeners();
            }
            catch (Exception ex)
            {
                Logging.Fatal(ex, nameof(ManzaTools), nameof(OnConfigParsed));
            }

            Responses.ReplyToServer("ConfigParsed ManzaTools", false, true);
        }

        private void InitBots()
        {
            AddCommand("css_bot", "Places a bot with given params", (player, info) => _botService.CreateBot(player, info));
            AddCommand("css_bot_kick", "Removes a single bots", (player, info) => _botService.RemoveBot(player, info));
            AddCommand("css_bots_kick", "Removes all bots", (player, info) => _botService.RemoveBots(player, info));
            RegisterEventHandler<EventPlayerSpawn>((@event, info) => _botService.PositionBotOnRespawn(@event, info));
        }

        private void InitChangeMap()
        {
            AddCommand("css_changeMap", "Changes the current Map", (player, info) => _changeMapService.Changemap(player, info, Config.AvailableMaps));
            RegisterListener((Listeners.OnMapStart)(entity => _gameModeService.LoadGameMode(GameModeEnum.Practice)));
        }

        private void InitClear()
        {
            AddCommand("css_clear", "Clears all Smoke, flying molotovs and fires", (player, info) => _clearService.ClearUtilities(player, info));
        }

        private void InitDeathMatch()
        {
            AddCommand("css_deathmatch", "Changes the current GameMode to deathmatch", (player, info) => _deathmatchService.StartDeathmatch(player, info));
            RegisterEventHandler<EventPlayerDeath>((@event, info) => _deathmatchService.HandlePlayerDeath(@event, info));
        }

        private void InitEffectTimers()
        {
            RegisterListener((Listeners.OnEntitySpawned)(entity => this._effectService.OnEntitySpawn(entity)));
            RegisterEventHandler<EventSmokegrenadeDetonate>((@event, info) => this._effectService.OnSmokeGrenadeDetonate(@event, info));
            RegisterEventHandler<EventPlayerBlind>((@event, info) => this._effectService.OnPlayerBlind(@event, info));
            RegisterEventHandler<EventPlayerHurt>((@event, info) => this._effectService.OnPlayerDamage(@event, info));
            AddCommand("css_smoketimer", "Toggles the SmokeTimer", (player, info) => this._effectService.ToggleSmokeTimer(player, info));
            AddCommand("css_blindtimer", "Toggles the BlindTimer", (player, info) => this._effectService.ToggleBlindTimerTimer(player, info));
            AddCommand("css_damageReport", "Toggles the DamageReport", (player, info) => this._effectService.ToggleDamageReport(player, info));
        }

        private void InitEndRound()
        {
            AddCommand("css_endround", "Ends a round in a PractiveMatch", (player, info) => _endRoundService.EndRound(player, info));
        }

        private void InitPractice()
        {
            AddCommand("css_prac", "Changes the current GameMode to practice", (player, info) => _gameModeService.LoadGameMode(GameModeEnum.Practice));
        }

        private void InitPracticeMatch()
        {
            AddCommand("css_pracmatch", "Changes the current GameMode to practice match", (player, info) => _gameModeService.LoadGameMode(GameModeEnum.PracticeMatch));
        }

        private void InitRcon()
        {
            AddCommand("css_rcon", "Executes a command on the server", (player, info) => _rconService.Execute(player, info));
        }

        private void InitRethrow()
        {
            AddCommand("css_rethrow", "Rethrows the last thrown grenade on the Server", (player, info) => _rethrowService.Rethrow(player, info));
        }

        private void InitSavedNades()
        {
            AddCommand("css_listnade", "Lists all saved Nades", (player, info) => _savedNadesService.ListNades(player, info));
            AddCommand("css_loadnade", "Loads a saved Nades", (player, info) => _savedNadesService.LoadNade(player, info));
            AddCommand("css_savenade", "Saves a saved nade", (player, info) => _savedNadesService.SaveNade(player, info));
            AddCommand("css_deletenade", "Delets a saved nade", (player, info) => _savedNadesService.DeleteNade(player, info));
            AddCommand("css_updatenade", "Updates a saved nade", (player, info) => _savedNadesService.UpdateNade(player, info));
        }

        private void InitSpawn()
        {
            AddCommand("css_spawn", "Sets the spawn of a player", (player, info) => _spawnService.SetPlayerPosition(player, info));
        }

        private void InitTestPlugin()
        {
            AddCommand("css_testplugin", "Tests if the plugin is running", (player, info) => Responses.ReplyToPlayer("Läuft", player));
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
            InitPractice();
            InitPracticeMatch();
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

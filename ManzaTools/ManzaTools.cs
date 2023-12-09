using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Config;
using CounterStrikeSharp.API.Modules.Entities;
using ManzaTools.Config;
using ManzaTools.Models;
using ManzaTools.Services;
using ManzaTools.Utils;

namespace ManzaTools
{
    public partial class ManzaTools : BasePlugin, IPluginConfig<ManzaToolsConfig>
    {
        public override string ModuleName => "ManzaTools";
        public override string ModuleAuthor => "DrOetcker";
        public override string ModuleVersion => "0.1.0";

        public ManzaToolsConfig Config { get; set; } = new();
        private readonly CfgShipperService _cfgShipper;
        private readonly GameModeService _gameModeService;
        private readonly SmokeTimerService _smokeTimer;
        private readonly ChangeMapService _changeMapService;
        private readonly DeathmatchService _deathmatchService;
        private readonly SpawnService _spawnService;
        private readonly ClearService _clearService;
        private readonly RconService _rconService;
        private readonly RethrowService _rethrowService;
        private readonly EndRoundService _endRoundService;
        private readonly SavedNadesService _savedNadesService;
        private readonly BotService _botService;

        public ManzaTools(CfgShipperService cfgShipper,
            GameModeService gameModeService,
            SmokeTimerService smokeTimer,
            ChangeMapService changeMapService,
            DeathmatchService deathmatchService,
            SpawnService spawnService,
            ClearService clearService,
            RconService rconService,
            RethrowService rethrowService,
            EndRoundService endRoundService,
            SavedNadesService savedNadesService,
            BotService botService
            )
        {
            _cfgShipper = cfgShipper;
            _gameModeService = gameModeService;
            _smokeTimer = smokeTimer;
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

        public override void Load(bool hotReload)
        {
            _cfgShipper.InitDefaultCfgs(ModuleDirectory);
            Responses.ReplyToServer("Loaded ManzaTools", false, true);
        }

        public override void Unload(bool hotReload)
        {
            Responses.ReplyToServer("Unloaded ManzaTools", false, true);
        }

        public void OnConfigParsed(ManzaToolsConfig config)
        {
            try
            {
                Config = config;
                Config.AvailibleMaps = _changeMapService.LoadMaps();
                _smokeTimer.SetSmokeTimerEnabled(Config.SmokeTimerEnabled);
                if (Config.DefaultGameMode != GameModeEnum.Disabled) 
                    RegisterListeners();
            }
            catch (Exception ex)
            {
                Logging.Log($"Error {ex.Message}, {ex.StackTrace}");
            }

            Responses.ReplyToServer("ConfigParsed ManzaTools", false, true);
        }

        private void RegisterListeners()
        {
            InitTestPlugin();
            InitSmokeTimer();
            InitChangeMap();
            InitPractice();
            InitPracticeMatch();
            InitDeathMatch();
            InitSpawn();
            InitClear();
            InitRcon();
            InitRethrow();
            InitEndround();
            InitSavedNades();
            InitBots();
        }

        private void InitPractice()
        {
            AddCommand("css_prac", "Changes the current GameMode to practice", (player, info) => _gameModeService.LoadGameMode(GameModeEnum.Practice));
        }

        private void InitPracticeMatch()
        {
            AddCommand("css_pracmatch", "Changes the current GameMode to practice match", (player, info) => _gameModeService.LoadGameMode(GameModeEnum.PracticeMatch));
        }

        private void InitDeathMatch()
        {
            AddCommand("css_deathmatch", "Changes the current GameMode to deathmatch", (player, info) => _deathmatchService.StartDeathmatch(player, info));
            RegisterEventHandler<EventPlayerDeath>((@event, info) => _deathmatchService.HandlePlayerDeath(@event, info));
        }

        private void InitChangeMap()
        {
            AddCommand("css_changeMap", "Changes the current Map", (player, info) => _changeMapService.Changemap(player, info, Config.AvailibleMaps));
            RegisterListener((Listeners.OnMapStart)(entity => _gameModeService.LoadGameMode(GameModeEnum.Practice)));
        }

        private void InitClear()
        {
            AddCommand("css_clear", "Clears all Smoke, flying molotovs and fires", (player, info) => _clearService.ClearUtilities(player, info));
        }

        private void InitRethrow()
        {
            AddCommand("css_rethrow", "Rethrows the last thrown grenade on the Server", (player, info) => _rethrowService.Rethrow(player, info));
        }

        private void InitSpawn()
        {
            AddCommand("css_spawn", "Sets the spawn of a player", (player, info) => _spawnService.SetPlayerPosition(player, info));
        }

        private void InitSmokeTimer()
        {
            RegisterListener((Listeners.OnEntitySpawned)(entity => _smokeTimer.OnEntitySpawn(entity)));
            RegisterEventHandler<EventSmokegrenadeDetonate>((@event, info) => _smokeTimer.OnSmokeGrenadeDetonate(@event, info));
            AddCommand("css_smoketimer", "Toggles the SmokeTimer", (player, info) => _smokeTimer.ToggleSmokeTimer(player, info));
        }

        private void InitSavedNades()
        {
            AddCommand("css_listnade", "Lists all saved Nades", (player, info) => _savedNadesService.ListNades(player, info));
            AddCommand("css_loadnade", "Loads a saved Nades", (player, info) => _savedNadesService.LoadNade(player, info));
            AddCommand("css_savenade", "Saves a saved nade", (player, info) => _savedNadesService.SaveNade(player, info));
            AddCommand("css_deletenade", "Delets a saved nade", (player, info) => _savedNadesService.DeleteNade(player, info));
            AddCommand("css_updatenade", "Updates a saved nade", (player, info) => _savedNadesService.UpdateNade(player, info));
        }

        private void InitEndround()
        {
            AddCommand("css_endround", "Ends a round in a PractiveMatch", (player, info) => _endRoundService.EndRound(player, info));
        }

        private void InitBots()
        {
            AddCommand("css_bot", "Places a bot with given params", (player, info) => _botService.CreateBot(player, info));
            RegisterEventHandler<EventPlayerSpawn>((@event, info) => _botService.PositionBotOnRespawn(@event, info));
        }

        private void InitRcon()
        {
            AddCommand("css_rcon", "Executes a command on the server", (player, info) => _rconService.Execute(player, info));
        }

        private void InitTestPlugin()
        {
            AddCommand("css_testplugin", "Tests if the plugin is running", (player, info) => Server.PrintToChatAll("Läuft"));
        }


    }
}

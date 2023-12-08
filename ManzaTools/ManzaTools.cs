using CounterStrikeSharp.API.Core;
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
        private readonly CfgShipper _cfgShipper;
        private readonly GameModeService _gameModeService;
        private readonly SmokeTimer _smokeTimer;
        private readonly ChangeMapService _changeMapService;
        private readonly DeathmatchService _deathmatchService;

        public ManzaTools(CfgShipper cfgShipper, 
            GameModeService gameModeService, 
            SmokeTimer smokeTimer, 
            ChangeMapService changeMapService, 
            DeathmatchService deathmatchService)
        {
            _cfgShipper = cfgShipper;
            _gameModeService = gameModeService;
            _smokeTimer = smokeTimer;
            _changeMapService = changeMapService;
            _deathmatchService = deathmatchService;
        }

        public override void Load(bool hotReload)
        {
            _cfgShipper.InitDefaultCfgs(ModuleDirectory);
            _gameModeService.LoadGameMode(_gameModeService.currentGameMode);
            RegisterListeners();
            Responses.ReplyToServer("Loaded ManzaTools", false, true);
        }

        public override void Unload(bool hotReload)
        {
            Responses.ReplyToServer("Unloaded ManzaTools", false, true);
        }

        public void OnConfigParsed(ManzaToolsConfig config)
        {
            Config = config;
            Config.AvailibleMaps = _changeMapService.LoadMaps();
            Responses.ReplyToServer("ConfigParsed ManzaTools", false, true);
        }

        private void RegisterListeners()
        {
            InitSmokeTimer();
            InitChangeMap();
            InitPractice();
            InitPracticeMatch();
            InitDeathMatch();
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
        }

        private void InitChangeMap()
        {
            AddCommand("css_changeMap", "Changes the current Map", (player, info) => _changeMapService.Changemap(player, info, Config.AvailibleMaps));
            RegisterListener((Listeners.OnMapStart)(entity => _gameModeService.LoadGameMode(GameModeEnum.Practice)));            
        }

        private void InitSmokeTimer()
        {
            RegisterListener((Listeners.OnEntitySpawned)(entity => _smokeTimer.OnEntitySpawn(entity, Config.SmokeTimerEnabled)));
            RegisterEventHandler<EventSmokegrenadeDetonate>((@event, info) => _smokeTimer.OnSmokeGrenadeDetonate(@event, info, Config.SmokeTimerEnabled));
        }


    }
}

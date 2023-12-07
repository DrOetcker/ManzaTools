using CounterStrikeSharp.API.Core;
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
        public override string ModuleVersion => "0.0.1";

        public ManzaToolsConfig Config { get; set; } = new();
        private readonly CfgShipper _cfgShipper;
        private readonly GameModeService _gameModeService;
        private readonly SmokeTimer _smokeTimer;
        private readonly ChangeMapService _changeMapService;

        public ManzaTools(CfgShipper cfgShipper, GameModeService gameModeService, SmokeTimer smokeTimer, ChangeMapService changeMapService)
        {
            _cfgShipper = cfgShipper;
            _gameModeService = gameModeService;
            _smokeTimer = smokeTimer;
            _changeMapService = changeMapService;
        }

        public override void Load(bool hotReload)
        {
            _cfgShipper.InitDefaultCfgs(ModuleDirectory);
            _gameModeService.LoadGameMode(GameModeEnum.Practice);
            RegisterListeners();
        }

        public override void Unload(bool hotReload)
        {
        }

        public void OnConfigParsed(ManzaToolsConfig config)
        {
            Config = config;
            Config.AvailibleMaps = _changeMapService.LoadMaps();
            Responses.ReplyToServer(">>> ConfigParsed ManzCs2Plugin <<<", false, true);
        }

        private void RegisterListeners()
        {
            InitSmokeTimer();
            InitChangeMap();
        }

        private void InitChangeMap()
        {
            AddCommand("css_changeMap", "Changes the current Map", (player, info) => _changeMapService.Changemap(player, info, Config.AvailibleMaps));
            //RegisterListener((Listeners.OnMapStart)(entity => Logging.Log("mapstart")));
        }

        private void InitSmokeTimer()
        {
            RegisterListener((Listeners.OnEntitySpawned)(entity => _smokeTimer.OnEntitySpawn(entity, Config.SmokeTimerEnabled)));
            RegisterEventHandler<EventSmokegrenadeDetonate>((@event, info) => _smokeTimer.OnSmokeGrenadeDetonate(@event, info, Config.SmokeTimerEnabled));
        }


    }
}

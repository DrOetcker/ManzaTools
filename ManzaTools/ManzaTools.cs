using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
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

        public ManzaTools(CfgShipper cfgShipper, GameModeService gameModeService, SmokeTimer smokeTimer)
        {
            _cfgShipper = cfgShipper;
            _gameModeService = gameModeService;
            _smokeTimer = smokeTimer;
        }

        public override void Load(bool hotReload)
        {
            _cfgShipper.InitDefaultCfgs(ModuleDirectory);
            if (!hotReload)
                _gameModeService.LoadGameMode(GameModeEnum.Practice);
            RegisterListeners();
        }

        public override void Unload(bool hotReload)
        {
        }

        public void OnConfigParsed(ManzaToolsConfig config)
        {
            Config = config;
            Responses.ReplyToServer(">>> ConfigParsed ManzCs2Plugin <<<", false, true);
        }

        private void RegisterListeners()
        {
            InitSmokeTimer();
        }

        private void InitSmokeTimer()
        {
            RegisterListener((Listeners.OnEntitySpawned)(entity => _smokeTimer.OnEntitySpawn(entity, Config.SmokeTimerEnabled)));
            RegisterEventHandler<EventSmokegrenadeDetonate>((@event, info) =>  _smokeTimer.OnSmokeGrenadeDetonate(@event, info, Config.SmokeTimerEnabled));
        }


    }
}

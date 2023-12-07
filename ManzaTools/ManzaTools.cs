using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using ManzaTools.Config;
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
        private CfgShipper _cfgShipper;

        public ManzaTools(CfgShipper cfgShipper)
        {
            _cfgShipper = cfgShipper;
        }

        public override void Load(bool hotReload)
        {
            _cfgShipper.InitDefaultCfgs(ModuleDirectory);
        }

        public override void Unload(bool hotReload)
        {

        }

        public void OnConfigParsed(ManzaToolsConfig config)
        {
            Config = config;
            Responses.ReplyToServer(">>> ConfigParsed ManzCs2Plugin <<<", false, true);
        }


    }
}

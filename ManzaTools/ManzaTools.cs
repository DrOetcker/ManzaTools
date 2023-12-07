using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace ManzaTools
{
    public class ManzaTools : BasePlugin, IPluginConfig<ManzaToolsConfig>
    {
        public override string ModuleName => "plugin name";
        public override string ModuleAuthor => "author";
        public override string ModuleVersion => "0.0.1";


        public ManzaToolsConfig Config { get; set; } = new();

        public void Load(bool hotReload)
        {

        }

        public void Unload(bool hotReload)
        {

        }

        public void OnConfigParsed(ManzaToolsConfig config)
        {
            this.Config = config;
        }
    }

}

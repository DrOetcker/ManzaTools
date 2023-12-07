using CounterStrikeSharp.API.Core;
using ManzaTools.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ManzaTools
{
    public class ManzaToolsServiceCollection : IPluginServiceCollection<ManzaTools>
    {
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<CfgShipper>();
            serviceCollection.AddSingleton<GameModeService>();
            serviceCollection.AddSingleton<SmokeTimer>();
        }
    }
}

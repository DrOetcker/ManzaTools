using CounterStrikeSharp.API.Core;
using ManzaTools.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ManzaTools
{
    public class ManzaToolsServiceCollection : IPluginServiceCollection<ManzaTools>
    {
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<CfgShipperService>();
            serviceCollection.AddSingleton<GameModeService>();
            serviceCollection.AddSingleton<SmokeTimerService>();
            serviceCollection.AddSingleton<ChangeMapService>(); 
            serviceCollection.AddSingleton<DeathmatchService>(); 
            serviceCollection.AddSingleton<SpawnService>();
            serviceCollection.AddSingleton<ClearService>();
            serviceCollection.AddSingleton<RconService>();
            serviceCollection.AddSingleton<RethrowService>();
        }
    }
}

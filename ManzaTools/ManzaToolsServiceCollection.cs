using CounterStrikeSharp.API.Core;

using ManzaTools.Interfaces;
using ManzaTools.Services;

using Microsoft.Extensions.DependencyInjection;

namespace ManzaTools
{
    public class ManzaToolsServiceCollection : IPluginServiceCollection<ManzaTools>
    {
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ICfgShipperService, CfgShipperService>();
            serviceCollection.AddSingleton<IGameModeService, GameModeService>();
            serviceCollection.AddSingleton<IEffectService, EffectService>();
            serviceCollection.AddSingleton<IChangeMapService, ChangeMapService>();
            serviceCollection.AddSingleton<IDeathmatchService, DeathmatchService>();
            serviceCollection.AddSingleton<ISpawnService, SpawnService>();
            serviceCollection.AddSingleton<IClearService, ClearService>();
            serviceCollection.AddSingleton<IRconService, RconService>();
            serviceCollection.AddSingleton<IRethrowService, RethrowService>();
            serviceCollection.AddSingleton<IEndRoundService, EndRoundService>();
            serviceCollection.AddSingleton<ISavedNadesService, SavedNadesService>();
            serviceCollection.AddSingleton<IBotService, BotService>();
            serviceCollection.AddSingleton<IRecordService, RecordService>();
            serviceCollection.AddSingleton<IAdminService, AdminService>();
        }
    }
}

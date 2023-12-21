using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

using ManzaTools.Interfaces;

using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class ClearService : PracticeBaseService, IClearService
    {
        public ClearService(ILogger<ClearService> logger, IGameModeService gameModeService)
            : base(logger, gameModeService)
        {
        }

        public override void AddCommands(Action<string, string, CommandInfo.CommandCallback> addCommand)
        {
            addCommand("css_clear", "Clears all Smoke, flying molotovs and fires", this.ClearUtilities);
        }

        public void ClearUtilities(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice)
                return;
            var smokes = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("smokegrenade_projectile");
            foreach (var smoke in smokes)
                smoke.Remove();

            var mollys = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("molotov_projectile");
            foreach (var molly in mollys)
                molly.Remove();

            var fires = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("inferno");
            foreach (var fire in fires)
                fire.Remove();
        }
    }
}

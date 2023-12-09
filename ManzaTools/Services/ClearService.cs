using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Services
{
    public class ClearService : PracticeBaseService
    {
        public ClearService(GameModeService gameModeService)
            : base(gameModeService)
        {
        }

        internal void ClearUtilities(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice)
                return;
            var smokes = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("smokegrenade_projectile");
            foreach (var smoke in smokes)
                smoke?.Remove();

            var mollys = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("molotov_projectile");
            foreach (var molly in mollys)
                molly?.Remove();

            var fires = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("inferno");
            foreach (var fire in fires)
                fire?.Remove();
        }
    }
}

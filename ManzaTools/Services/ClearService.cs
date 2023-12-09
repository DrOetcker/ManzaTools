using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Services
{
    public class ClearService
    {
        private readonly GameModeService _gameModeService;

        public ClearService(GameModeService gameModeService)
        {
            _gameModeService = gameModeService;
        }

        internal void SetPlayerPosition(CCSPlayerController? player, CommandInfo info)
        {
            if (!_gameModeService.IsPractice())
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

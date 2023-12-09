using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using ManzaTools.Utils;
using Serilog.Core;

namespace ManzaTools.Services
{
    public class SpawnService
    {
        private readonly GameModeService _gameModeService;

        //TeamNum = 3
        public IList<SpawnPoint> CtSpawns { get; }

        //TeamNum = 2
        public IList<SpawnPoint> TSpawns { get; }

        public SpawnService(GameModeService gameModeService)
        {
            _gameModeService = gameModeService;
            CtSpawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist").Where(x => x.IsValid && x.Enabled && x.Priority == 0).ToList();
            TSpawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist").Where(x => x.IsValid && x.Enabled && x.Priority == 0).ToList();
        }

        internal void SetPlayerPosition(CCSPlayerController? player, CommandInfo info)
        {
            if (!_gameModeService.IsPractice())
                return;

            if (!int.TryParse(info.ArgByIndex(1), out int spawnId) && spawnId >= 1)
            {
                Responses.ReplyToPlayer($"Usage: !spawn 1-5 t/ct -> !spawn 2 ct", player, true);
                return;
            }
            var teamArg = info.ArgByIndex(2);
            var teamNum = string.IsNullOrEmpty(teamArg) ? player.TeamNum : (teamArg.ToLower() == "t" ? 2 : 3);
            var spawn = teamNum == 2 ? TSpawns[spawnId - 1] : CtSpawns[spawnId - 1];
            if(spawn.CBodyComponent?.SceneNode != null && player?.PlayerPawn?.Value != null)
            {
                player.PlayerPawn.Value.Teleport(spawn.CBodyComponent.SceneNode.AbsOrigin, spawn.CBodyComponent.SceneNode.AbsRotation, new Vector(0, 0, 0));
            }

        }
    }
}

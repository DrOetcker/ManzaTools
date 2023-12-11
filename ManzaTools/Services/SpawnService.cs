using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

using ManzaTools.Extensions;
using ManzaTools.Interfaces;
using ManzaTools.Utils;

using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class SpawnService : PracticeBaseService, ISpawnService
    {
        public SpawnService(ILogger<SpawnService> logger, IGameModeService gameModeService)
            : base(logger, gameModeService)
        {
        }

        public void SetPlayerPosition(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player == null)
                return;

            var ctSpawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist").Where(x => x.IsValid && x.Enabled && x.Priority == 0).ToList();
            var tSpawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist").Where(x => x.IsValid && x.Enabled && x.Priority == 0).ToList();

            if (!int.TryParse(info.ArgByIndex(1), out var spawnId) && spawnId >= 1)
            {
                Responses.ReplyToPlayer("Usage: !spawn 1-5 t/ct -> !spawn 2 ct", player, true);
                return;
            }
            var teamArg = info.ArgByIndex(2);
            var teamNum = string.IsNullOrEmpty(teamArg) ? player.TeamNum : (teamArg.ToLower() == "t" ? (byte)2 : (byte)3);
            var spawn = PlayerExtension.IsCounterTerrorist(teamNum) ? ctSpawns[spawnId - 1] : tSpawns[spawnId - 1];
            if (spawn.CBodyComponent?.SceneNode != null && player.PlayerPawn.Value != null)
                player.PlayerPawn.Value.Teleport(spawn.CBodyComponent.SceneNode.AbsOrigin, spawn.CBodyComponent.SceneNode.AbsRotation, new Vector(0, 0, 0));

        }
    }
}

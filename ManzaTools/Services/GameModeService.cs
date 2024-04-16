using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using ManzaTools.Interfaces;
using ManzaTools.Models;
using ManzaTools.Utils;

using Microsoft.Extensions.Logging;
using System.Drawing;

namespace ManzaTools.Services
{
    public class GameModeService : BaseService, IGameModeService
    {
        public GameModeEnum CurrentGameMode { get; private set; }

        public GameModeService(ILogger<GameModeService> logger)
            : base(logger)
        {
        }

        public override void Init(ManzaTools manzaTools)
        {
            manzaTools.AddCommand("css_prac", "Changes the current GameMode to practice", (player, info) => LoadGameMode(GameModeEnum.Practice));
            manzaTools.AddCommand("css_pracmatch", "Changes the current GameMode to practice match", (player, info) => LoadGameMode(GameModeEnum.PracticeMatch));
            manzaTools.AddCommand("css_match", "Changes the current GameMode to match match", (player, info) => LoadGameMode(GameModeEnum.Match));
        }


        public bool IsPractice()
        {
            return CurrentGameMode == GameModeEnum.Practice || CurrentGameMode == GameModeEnum.PracticeMatch;
        }

        public void LoadGameMode(GameModeEnum newGameMode)
        {
            var cfgToLoad = Statics.GameModeCfgs[newGameMode];
            if (string.IsNullOrEmpty(cfgToLoad))
            {
                _logger.LogError($"No cfg found for GameMode {newGameMode}. Keeping GameMode {CurrentGameMode}");
                return;
            }
            if (newGameMode == GameModeEnum.PracticeMatch)
            {
                Server.ExecuteCommand($"execifexists {Path.Combine("ManzaTools", Statics.GameModeCfgs[GameModeEnum.Practice])}");
            }
            Server.ExecuteCommand($"execifexists {Path.Combine("ManzaTools", cfgToLoad)}");
            CurrentGameMode = newGameMode;
            Responses.ReplyToServer($"Loaded GameMode {CurrentGameMode}!{GetHappyTextByMode(CurrentGameMode)}");
            DrawSpawns();
        }

        private string GetHappyTextByMode(GameModeEnum currentGameMode)
        {
            switch (currentGameMode)
            {
                case GameModeEnum.Practice:
                    return " Happy smoking! May I get a drag from that spliff too?";
                case GameModeEnum.PracticeMatch:
                    return " VOLKER! 10seconds! WAS MACHEN WIR???";
                case GameModeEnum.Deathmatch:
                    return " MOMOMOMONSTERKILLKILLKILLKILL";
                case GameModeEnum.Match:
                    return " Da Björni. Endlich zufriden? DEIN match-mode";
                default:
                    return string.Empty;
            }
        }

        public void DrawSpawns()
        {
            if (CurrentGameMode != GameModeEnum.Practice)
                return;

            var ctSpawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist").Where(x => x.IsValid && x.Enabled && x.Priority == 0).ToList();
            var tSpawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist").Where(x => x.IsValid && x.Enabled && x.Priority == 0).ToList();
            foreach (var spawn in ctSpawns)
            {
                DrawPolygon(new List<Vector> {
                    new Vector(spawn.CBodyComponent.SceneNode.AbsOrigin.X +10, spawn.CBodyComponent.SceneNode.AbsOrigin.Y+10, spawn.CBodyComponent.SceneNode.AbsOrigin.Z),
                    new Vector(spawn.CBodyComponent.SceneNode.AbsOrigin.X +10, spawn.CBodyComponent.SceneNode.AbsOrigin.Y-10, spawn.CBodyComponent.SceneNode.AbsOrigin.Z),
                    new Vector(spawn.CBodyComponent.SceneNode.AbsOrigin.X -10, spawn.CBodyComponent.SceneNode.AbsOrigin.Y-10, spawn.CBodyComponent.SceneNode.AbsOrigin.Z),
                    new Vector(spawn.CBodyComponent.SceneNode.AbsOrigin.X -10, spawn.CBodyComponent.SceneNode.AbsOrigin.Y+10, spawn.CBodyComponent.SceneNode.AbsOrigin.Z),
                }, 1, Color.MediumVioletRed);
            }
            foreach (var spawn in tSpawns)
            {
                DrawPolygon(new List<Vector> {
                    new Vector(spawn.CBodyComponent.SceneNode.AbsOrigin.X +10, spawn.CBodyComponent.SceneNode.AbsOrigin.Y+10, spawn.CBodyComponent.SceneNode.AbsOrigin.Z),
                    new Vector(spawn.CBodyComponent.SceneNode.AbsOrigin.X +10, spawn.CBodyComponent.SceneNode.AbsOrigin.Y-10, spawn.CBodyComponent.SceneNode.AbsOrigin.Z),
                    new Vector(spawn.CBodyComponent.SceneNode.AbsOrigin.X -10, spawn.CBodyComponent.SceneNode.AbsOrigin.Y-10, spawn.CBodyComponent.SceneNode.AbsOrigin.Z),
                    new Vector(spawn.CBodyComponent.SceneNode.AbsOrigin.X -10, spawn.CBodyComponent.SceneNode.AbsOrigin.Y+10, spawn.CBodyComponent.SceneNode.AbsOrigin.Z),
                }, 1, Color.MediumVioletRed);
            }
        }


        public static void DrawPolygon(List<Vector> vertices, float width, Color color)
        {
            if (vertices.Count < 2)
            {
                return;
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                int nextIndex = (i + 1) % vertices.Count;
                DrawBeam(vertices[i], vertices[nextIndex], width, color);
            }
        }
        public static void DrawBeam(Vector startPos, Vector endPos, float width, Color color)
        {
            CBeam beam = Utilities.CreateEntityByName<CBeam>("beam");
            if (beam == null)
            {
                // Failed to create beam...  
                return;
            }

            beam.Render = color;
            beam.Width = width;
            beam.Teleport(startPos, new QAngle(), new Vector());
            beam.EndPos.Add(endPos);
            beam.DispatchSpawn();
        }
    }
}

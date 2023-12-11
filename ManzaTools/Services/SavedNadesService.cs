using System.Text.Json;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

using ManzaTools.Interfaces;
using ManzaTools.Models;
using ManzaTools.Utils;

using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class SavedNadesService : PracticeBaseService, ISavedNadesService
    {
        private static readonly string savedNadesFilePath = Path.Join(Statics.CfgPath, "savedNades.json");

        public SavedNadesService(ILogger<SavedNadesService> logger, IGameModeService gameModeService)
            : base(logger, gameModeService)
        {
        }

        public void DeleteNade(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player == null)
                return;

            if (info.ArgCount == 1)
            {
                Responses.ReplyToPlayer("Usage: !deletenade ID | NameOhneSpace | \"Name Mit Space\"", player, true);
                return;
            }
            var nadeName = info.ArgByIndex(1);
            var argIsId = uint.TryParse(nadeName, out var nadeId);

            var savedNades = GetExistingNades(Server.MapName);
            var savedNadeToDelete = argIsId ? savedNades.FirstOrDefault(x => x.Id == nadeId) : savedNades.FirstOrDefault(x => x.Name.ToLower() == nadeName.ToLower());

            if (savedNadeToDelete == null)
            {
                Responses.ReplyToPlayer($"Could not find nade {nadeName}", player, true);
                return;
            }

            savedNades.Remove(savedNadeToDelete);
            File.WriteAllText(savedNadesFilePath, JsonSerializer.Serialize(savedNades));

            Responses.ReplyToServer($"Nade {savedNadeToDelete.Name} (Id {savedNadeToDelete.Id}) deleted.", true);
            Responses.ReplyToServer("Details for one last time:");
            Responses.ReplyToServer($"Name: {savedNadeToDelete.Name}");
            Responses.ReplyToServer($"Type: {savedNadeToDelete.Type}");
            if (!string.IsNullOrEmpty(savedNadeToDelete.Description))
                Responses.ReplyToServer($"Description: {savedNadeToDelete.Description}");
            Responses.ReplyToServer($"Position: {savedNadeToDelete.PlayerPosition}");
            Responses.ReplyToServer($"Angle: {savedNadeToDelete.PlayerAngle}");
            Responses.ReplyToServer($"Teleport: setpos {savedNadeToDelete.PlayerPosition}; setang {savedNadeToDelete.PlayerAngle}");
            Responses.ReplyToServer($"Map: {savedNadeToDelete.Map}");
        }

        public void ListNades(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player == null)
                return;

            var existingNades = GetExistingNades(Server.MapName);
            if (!existingNades.Any())
            {
                Responses.ReplyToPlayer($"Could not find nades for {Server.MapName}", player, true);
                return;
            }

            if (info.ArgCount > 1)
            {
                var filterSubjects = info.ArgString.ToLower().Split(' ');
                var filterByType = existingNades.Where(x =>
                {
                    var variants = SimulateUserVariants(x.Type);
                    foreach (var variant in variants)
                        if (filterSubjects.Contains(variant))
                            return true;
                    return false;
                });
                var filterByName = existingNades.Where(x =>
                {
                    foreach (var filterSubject in filterSubjects)
                        if (x.Name.ToLower().Contains(filterSubject))
                            return true;
                    return false;

                });
                var filterByDesc = existingNades.Where(x =>
                {
                    foreach (var filterSubject in filterSubjects)
                        if (!string.IsNullOrEmpty(x.Description) && x.Description.ToLower().Contains(filterSubject))
                            return true;
                    return false;
                });

                existingNades = filterByType.Concat(filterByName).Concat(filterByDesc).ToList();
            }

            var orderedNades = existingNades.DistinctBy(x => x.Id).OrderBy(x => x.Type).ThenBy(x => x.Id).ToList();
            foreach (var nade in orderedNades)
            {
                Responses.ReplyToPlayer($"Id: {nade.Id}, {nade.Type}, {nade.Name}", player);
            }
            Responses.ReplyToPlayer($"Example loading: !loadnade 1", player);
            Responses.ReplyToPlayer($"Example loading: !loadnade \"God flash BSite\"", player);
        }

        public void LoadNade(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player?.PlayerPawn.Value == null)
                return;

            if (info.ArgCount == 1)
            {
                Responses.ReplyToPlayer("Usage: !loadnade ID | NameOhneSpace | \"Name Mit Space\"", player, true);
                return;
            }
            var nadeName = info.ArgByIndex(1);
            var argIsId = uint.TryParse(nadeName, out var nadeId);

            var savedNades = GetExistingNades(Server.MapName);
            var savedNade = argIsId ? savedNades.FirstOrDefault(x => x.Id == nadeId) : savedNades.FirstOrDefault(x => x.Name.ToLower() == nadeName.ToLower());

            if (savedNade == null)
            {
                Responses.ReplyToPlayer($"Could not find nade {nadeName}", player, true);
                return;
            }

            var nadeToLoadPlayerPos = GetVector(savedNade.PlayerPosition);
            var nadeToLoadPlayerAngle = GetAngle(savedNade.PlayerAngle);
            if (savedNade.IsImported)
            {
                nadeToLoadPlayerPos.Z -= 59;
                Responses.ReplyToPlayer($"Nade wurde aus Stratsbook importiert. Sollte das Lineup nicht passen, bitte an Oetcker melden!", player);
            }
            player.PlayerPawn.Value.Teleport(nadeToLoadPlayerPos, nadeToLoadPlayerAngle, new Vector(0, 0, 0));

            switch (savedNade.Type)
            {
                case Consts.Flash:
                    player.GiveNamedItem("weapon_flashbang");
                    player.ExecuteClientCommand("slot7");
                    break;
                case Consts.Smoke:
                    player.GiveNamedItem("weapon_smokegrenade");
                    player.ExecuteClientCommand("slot8");
                    break;
                case Consts.Nade:
                    player.GiveNamedItem("weapon_hegrenade");
                    player.ExecuteClientCommand("slot6");
                    break;
                case Consts.Molotov:
                    player.GiveNamedItem("weapon_molotov");
                    player.ExecuteClientCommand("slot10");
                    break;
            }
            Responses.ReplyToPlayer($"Nade {savedNade.Name} (Id {savedNade.Id}) loaded.", player);
            if (!string.IsNullOrEmpty(savedNade.Description))
            {
                Responses.ReplyToPlayer(savedNade.Description, player);
                player.PrintToCenter(savedNade.Description);
            }
        }

        public void SaveNade(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player?.PlayerPawn.Value == null || player.Pawn.Value == null)
                return;

            if (info.ArgCount == 1)
            {
                Responses.ReplyToPlayer("Usage: !savenade OhneSpace | \"Mit Space\"", player, true);
                return;
            }
            var nadeName = info.ArgByIndex(1);
            var description = info.ArgByIndex(2);
            var team = info.ArgByIndex(3);

            var playerName = player.PlayerName;
            var playerSteamId = player.SteamID.ToString();
            var playerAngle = player.PlayerPawn.Value.EyeAngles;
            var playerPos = player.Pawn.Value.CBodyComponent!.SceneNode!.AbsOrigin;
            var mapName = Server.MapName;
            var nadeType = GetNadeType(player.PlayerPawn.Value.WeaponServices?.ActiveWeapon.Value?.DesignerName);
            if (string.IsNullOrEmpty(nadeType))
            {
                Responses.ReplyToPlayer($"{player.PlayerPawn.Value.WeaponServices?.ActiveWeapon.Value?.DesignerName} not supported", player, true);
                return;
            }

            VerifySavedNadesFileExists();
            var savedNades = GetExistingNades();

            try
            {
                var nadeToSave = new SavedNade
                {
                    Id = GetIndexForNewNade(savedNades),
                    Map = mapName,
                    Type = nadeType,
                    CreatorSteamId = playerSteamId,
                    Creator = playerName,
                    PlayerPosition = $"{playerPos.X} {playerPos.Y} {playerPos.Z}",
                    PlayerAngle = $"{playerAngle.X} {playerAngle.Y} {playerAngle.Z}",
                    Name = nadeName,
                    Description = description,
                    Team = team
                };
                savedNades.Add(nadeToSave);
                File.WriteAllText(savedNadesFilePath, JsonSerializer.Serialize(savedNades));
                Responses.ReplyToPlayer($"Saved: Id: {nadeToSave.Id}, {nadeToSave.Type}, {nadeToSave.Name}, {nadeToSave.Description}", player);
            }
            catch (JsonException ex)
            {
                _logger.LogError("Could not save nade", ex);
            }
        }

        public void UpdateNade(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player == null)
                return;

            Responses.ReplyToPlayer("UpdateNade - Not implemented yet", player);
        }

        private static QAngle GetAngle(string playerAngle)
        {
            var coordinates = playerAngle.Split(' ');
            return new QAngle(float.Parse(coordinates[0]), float.Parse(coordinates[1]), float.Parse(coordinates[2]));
        }

        private static IList<SavedNade> GetExistingNades(string? mapName = null)
        {
            if (!File.Exists(savedNadesFilePath))
                return new List<SavedNade>();

            var existingNadesJson = File.ReadAllText(savedNadesFilePath);
            var savedNades = JsonSerializer.Deserialize<IList<SavedNade>>(existingNadesJson) ?? new List<SavedNade>();
            return string.IsNullOrEmpty(mapName) ?
                       savedNades :
                       savedNades.Where(x => x.Map == mapName).ToList();
        }

        private static uint GetIndexForNewNade(IEnumerable<SavedNade> savedNades)
        {
            var lastSavedNade = savedNades.LastOrDefault();
            return lastSavedNade != null ? lastSavedNade.Id + 1 : 0;
        }

        private static string GetNadeType(string? cs2NadeName)
        {
            switch (cs2NadeName)
            {
                case Consts.FlashCs2:
                    return Consts.Flash;
                case Consts.SmokeCs2:
                    return Consts.Smoke;
                case Consts.NadeCs2:
                    return Consts.Nade;
                case Consts.MolotovCs2:
                case Consts.IncCs2:
                    return Consts.Molotov;
                default:
                    return "";
            }
        }

        private static Vector GetVector(string playerPosition)
        {
            var coordinates = playerPosition.Split(' ');
            return new Vector(float.Parse(coordinates[0]), float.Parse(coordinates[1]), float.Parse(coordinates[2]));
        }

        private static IEnumerable<string> SimulateUserVariants(string? type)
        {
            switch (type)
            {
                case Consts.Nade:
                    return new List<string> { "nade", "he", "grenade", "granate" };
                case Consts.Flash:
                    return new List<string> { "flash", "flashbang", "blend" };
                case Consts.Smoke:
                    return new List<string> { "smoke", "rauch" };
                case Consts.Molotov:
                    return new List<string> { "molotov", "molly", "brand", "feuer" };
                default: return new List<string>();
            }
        }

        private static void VerifySavedNadesFileExists()
        {
            if (!File.Exists(savedNadesFilePath))
                File.WriteAllText(savedNadesFilePath, "[]");
        }
    }
}

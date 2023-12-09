using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using ManzaTools.Models;
using ManzaTools.Utils;
using System.Text.Json;

namespace ManzaTools.Services
{
    public class SavedNadesService : PracticeBaseService
    {
        private static string savedNadesFilePath = Path.Join(Statics.CfgPath, "savedNades.json");
        public SavedNadesService(GameModeService gameModeService)
            : base(gameModeService)
        {
        }

        internal void ListNades(CCSPlayerController? player, CommandInfo info)
        {
            var savednades = GetExistingNades(Server.MapName);
            if (!savednades.Any())
            {
                Responses.ReplyToPlayer($"Could not find nades for {Server.MapName}", player, true);
                return;
            }

            if (info.ArgCount > 1)
            {
                var filterSubjects = info.ArgString.ToLower().Split(' ');
                var filterByType = savednades.Where(x =>
                {
                    var variants = SimulateUserVariants(x.Type);
                    foreach (var variant in variants)
                        if (filterSubjects.Contains(variant))
                            return true;
                    return false;
                });
                var filterByName = savednades.Where(x =>
                {
                    foreach (var filterSubject in filterSubjects)
                        if (x.Name.ToLower().Contains(filterSubject))
                            return true;
                    return false;

                });
                var filterByDesc = savednades.Where(x =>
                {
                    foreach (var filterSubject in filterSubjects)
                        if (x.Description.ToLower().Contains(filterSubject))
                            return true;
                    return false;
                });

                savednades = filterByType.Concat(filterByName).Concat(filterByDesc).ToList();
            }

            var orderedNades = savednades.DistinctBy(x => x.Id).OrderBy(x => x.Type).ThenBy(x => x.Id).ToList();
            foreach (var nade in orderedNades)
            {
                Responses.ReplyToPlayer($"Id: {nade.Id}, {nade.Type}, {nade.Name}", player);
            }
            Responses.ReplyToPlayer($"Example loading: !loadnade 1", player);
            Responses.ReplyToPlayer($"Example loading: !loadnade \"God flash BSite\"", player);
        }

        internal void SaveNade(CCSPlayerController? player, CommandInfo info)
        {
            if (info.ArgCount == 1)
            {
                Responses.ReplyToPlayer("Usage: !savenade OhneSpace | \"Mit Space\"", player, true);
                return;
            }
            var nadeName = info.ArgByIndex(1);
            var description = info.ArgByIndex(2);
            var team = info.ArgByIndex(3);

            string playerName = player.PlayerName;
            string playerSteamID = player.SteamID.ToString();
            QAngle playerAngle = player.PlayerPawn.Value.EyeAngles;
            Vector playerPos = player.Pawn.Value.CBodyComponent!.SceneNode!.AbsOrigin;
            string mapName = Server.MapName;
            string nadeType = GetNadeType(player.PlayerPawn.Value.WeaponServices?.ActiveWeapon.Value.DesignerName);
            if (string.IsNullOrEmpty(nadeType))
            {
                Responses.ReplyToPlayer($"{player.PlayerPawn.Value.WeaponServices?.ActiveWeapon.Value.DesignerName} not supported", player, true);
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
                    CreatorSteamId = playerSteamID,
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
                Logging.Log($"Error handling JSON: {ex.Message}");
            }
        }

        internal void LoadNade(CCSPlayerController? player, CommandInfo info)
        {
            if (info.ArgCount == 1)
            {
                Responses.ReplyToPlayer("Usage: !loadnade ID | NameOhneSpace | \"Name Mit Space\"", player, true);
                return;
            }
            var nadeName = info.ArgByIndex(1);
            var argIsId = uint.TryParse(nadeName, out var nadeId);

            var savedNades = GetExistingNades(Server.MapName);
            SavedNade? savedNade;
            if (argIsId)
                savedNade = savedNades.FirstOrDefault(x => x.Id == nadeId);
            else
                savedNade = savedNades.FirstOrDefault(x => x.Name.ToLower() == nadeName.ToLower());

            if (savedNade == null)
            {
                Responses.ReplyToPlayer($"Could not find nade {nadeName}", player, true);
                return;
            }

            Vector nadeToLoadPlayerPos = GetVector(savedNade.PlayerPosition);
            QAngle nadeToLoadPlayerAngle = GetAngle(savedNade.PlayerAngle);
            if (savedNade.IsImported)
            {
                nadeToLoadPlayerPos.Z -= 59;
                Responses.ReplyToPlayer($"Nade wurde aus Stratsbook importiert. Sollte das Lineup nicht passen, bitte an Oetcker melden!", player);
            }
            player.PlayerPawn.Value.Teleport(nadeToLoadPlayerPos, nadeToLoadPlayerAngle, new Vector(0,0,0));

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

        internal void DeleteNade(CCSPlayerController? player, CommandInfo info)
        {
            Responses.ReplyToPlayer("DeleteNade", player);
        }

        internal void UpdateNade(CCSPlayerController? player, CommandInfo info)
        {
            Responses.ReplyToPlayer("UpdateNade", player);
        }

        private IList<SavedNade> GetExistingNades(string? mapName = null)
        {
            if(!File.Exists(savedNadesFilePath)) 
                return new List<SavedNade>();

            string existingNadesJson = File.ReadAllText(savedNadesFilePath);
            IList<SavedNade> savedNades = JsonSerializer.Deserialize<IList<SavedNade>>(existingNadesJson) ?? new List<SavedNade>();
            if (string.IsNullOrEmpty(mapName))
                return savedNades;
            else
                return savedNades.Where(x => x.Map == mapName).ToList();
        }

        private IList<string> SimulateUserVariants(string type)
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

        private static QAngle GetAngle(string playerAngle)
        {
            var coordinates = playerAngle.Split(' ');
            return new QAngle(float.Parse(coordinates[0]), float.Parse(coordinates[1]), float.Parse(coordinates[2]));
        }

        private static Vector GetVector(string playerPosition)
        {
            var coordinates = playerPosition.Split(' ');
            return new Vector(float.Parse(coordinates[0]), float.Parse(coordinates[1]), float.Parse(coordinates[2]));
        }

        private void VerifySavedNadesFileExists()
        {
            if (!File.Exists(savedNadesFilePath))
                File.WriteAllText(savedNadesFilePath, "[]");
        }

        private string GetNadeType(string? cs2nadeName)
        {
            switch (cs2nadeName)
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

        private uint GetIndexForNewNade(IList<SavedNade> savedNades)
        {
            var lastSavedNade = savedNades.LastOrDefault();
            return lastSavedNade != null ? lastSavedNade.Id + 1 : 0;
        }
    }
}

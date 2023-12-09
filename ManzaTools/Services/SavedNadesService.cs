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
        private static string lineupFilePath = Path.Join(Statics.CfgPath, "savedNades.json");
        public SavedNadesService(GameModeService gameModeService)
            : base(gameModeService)
        {
        }

        internal void ListNades(CCSPlayerController? player, CommandInfo info)
        {
            var savednades = GetExistingLineups(Server.MapName);
            if (!savednades.Any())
            {
                Responses.ReplyToPlayer($"Could not find lineups for {Server.MapName}", player, true);
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

            var orderedLineups = savednades.DistinctBy(x => x.Id).OrderBy(x => x.Type).ThenBy(x => x.Id).ToList();
            foreach (var lineup in orderedLineups)
            {
                Responses.ReplyToPlayer($"Id: {lineup.Id}, {lineup.Type}, {lineup.Name}", player);
            }
            Responses.ReplyToPlayer($"Example loading: !loadlineup 1", player);
            Responses.ReplyToPlayer($"Example loading: !loadlineup \"God flash BSite\"", player);
        }

        internal void SaveNade(CCSPlayerController? player, CommandInfo info)
        {
            Responses.ReplyToPlayer("SaveNade", player);
        }

        internal void LoadNade(CCSPlayerController? player, CommandInfo info)
        {
            if (info.ArgCount == 1)
            {
                Responses.ReplyToPlayer("Usage: !loadlineup ID | NameOhneSpace | \"Name Mit Space\"", player, true);
                return;
            }
            var lineupName = info.ArgByIndex(1);
            var argIsId = uint.TryParse(lineupName, out var lineupId);

            var savedNades = GetExistingLineups(Server.MapName);
            SavedNade? savedNade;
            if (argIsId)
                savedNade = savedNades.FirstOrDefault(x => x.Id == lineupId);
            else
                savedNade = savedNades.FirstOrDefault(x => x.Name.ToLower() == lineupName.ToLower());

            if (savedNade == null)
            {
                Responses.ReplyToPlayer($"Could not find lineup {lineupName}", player, true);
                return;
            }

            Vector lineupToLoadPlayerPos = GetVector(savedNade.PlayerPosition);
            QAngle lineupToLoadPlayerAngle = GetAngle(savedNade.PlayerAngle);
            if (savedNade.IsImported)
            {
                lineupToLoadPlayerPos.Z -= 59;
                Responses.ReplyToPlayer($"Nade wurde aus Stratsbook importiert. Sollte das Lineup nicht passen, bitte an Oetcker melden!", player);
            }
            player.PlayerPawn.Value.Teleport(lineupToLoadPlayerPos, lineupToLoadPlayerAngle, new Vector(0,0,0));

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
            Responses.ReplyToPlayer($"Lineup {savedNade.Name} (Id {savedNade.Id}) loaded.", player);
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

        private IList<SavedNade> GetExistingLineups(string? mapName = null)
        {
            if(!File.Exists(lineupFilePath)) 
                return new List<SavedNade>();

            string existingLineupsJson = File.ReadAllText(lineupFilePath);
            IList<SavedNade> savedLineups = JsonSerializer.Deserialize<IList<SavedNade>>(existingLineupsJson) ?? new List<SavedNade>();
            if (string.IsNullOrEmpty(mapName))
                return savedLineups;
            else
                return savedLineups.Where(x => x.Map == mapName).ToList();
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
    }
}

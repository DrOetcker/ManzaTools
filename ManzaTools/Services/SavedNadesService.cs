using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
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
            Responses.ReplyToPlayer("LoadNade", player);
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
    }
}

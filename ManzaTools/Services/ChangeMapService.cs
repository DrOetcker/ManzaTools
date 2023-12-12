using System.Text.Json;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

using ManzaTools.Interfaces;
using ManzaTools.Models;
using ManzaTools.Utils;

using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class ChangeMapService : BaseService, IChangeMapService
    {
        public ChangeMapService(ILogger<ChangeMapService> logger)
            : base(logger)
        {
        }

        public void Changemap(CCSPlayerController? player, CommandInfo command, IList<Map> availableMaps)
        {
            if (player == null)
                return;

            if (command.ArgCount == 1)
            {
                Responses.ReplyToPlayer(Statics.GetChatText("Available Maps:"), player);
                Responses.ReplyToPlayer(string.Join(", ", availableMaps.Select(x => x.Name)), player);
                return;
            }
            var newMapName = command.GetArg(1);
            var newMap = availableMaps.FirstOrDefault(map => FindMapByName(map, newMapName));
            if (newMap == null)
            {
                Responses.ReplyToPlayer($"Selected map {newMapName} not found. Available Maps:", player, true);
                Responses.ReplyToPlayer(string.Join(", ", availableMaps.Select(x => x.Name)), player);
                Responses.ReplyToServer($"Selected map {newMapName} not found.", false, true);
            }
            else
            {
                if (newMap.Id > 0)
                    Server.ExecuteCommand($"host_workshop_map {newMap.Id}");
                else
                    Server.ExecuteCommand($"changelevel {newMap.Name}");
            }
        }

        private static bool FindMapByName(Map map, string newMapName)
        {
            bool mapFound = map.Name == newMapName;
            if (!mapFound)
            {
                return map.Name?.Substring(3) == newMapName;
            }
            return mapFound;
        }

        public IList<Map> PreLoadAvailableMaps()
        {
            var fileName = "maps.json";
            var filePath = Path.Join(Statics.CfgPath, fileName);
            if (File.Exists(filePath))
            {
                List<Map> loadedMaps = new();
                try
                {
                    using (var fileReader = File.OpenText(filePath))
                    {
                        var jsonContent = fileReader.ReadToEnd();
                        if (!string.IsNullOrEmpty(jsonContent))
                            loadedMaps = JsonSerializer.Deserialize<List<Map>>(jsonContent) ?? new List<Map>();

                    }
                    var maps = "Available Maps: \r\n";
                    foreach (var map in loadedMaps)
                        maps += $"{map.Name}{(map.Id != null ? $", WorkshopId: {map.Id}" : "")}\r\n";

                    _logger.LogInformation(maps);

                    return loadedMaps;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Could not load available maps", ex);
                    return new List<Map>();
                }
            }
            else
            {

                List<Map> defaultMaps = new List<Map>
                {
                    new() {Name = "de_ancient"},
                    new() {Name = "cs_italy"},
                    new() {Name = "cs_office"},
                    new() {Name = "cs_vertigo"},
                    new() {Name = "de_anubis"},
                    new() {Name = "de_dust2"},
                    new() {Name = "de_inferno"},
                    new() {Name = "de_mirage"},
                    new() {Name = "de_nuke"},
                    new() {Name = "de_overpass"},
                    new() {Name = "de_vertigo"}
                };
                try
                {
                    JsonSerializerOptions options = new()
                    {
                        WriteIndented = true,
                    };
                    var defaultJson = JsonSerializer.Serialize(defaultMaps, options);
                    var directoryPath = Path.GetDirectoryName(filePath);
                    if (directoryPath != null)
                        if (!Directory.Exists(directoryPath))
                            Directory.CreateDirectory(directoryPath);

                    File.WriteAllText(filePath, defaultJson);

                    _logger.LogInformation("Created a new JSON file with default maps.");
                    return defaultMaps;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Could not save default maps", ex);
                    return new List<Map>();
                }
            }

        }
    }
}

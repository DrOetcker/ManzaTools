using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using DrOetcker.Models;
using ManzaTools.Utils;
using System.Text.Json;

namespace ManzaTools.Services
{
    public class ChangeMapService
    {
        //TODO Catch MapChangedEvent & set GameMode
        internal void Changemap(CCSPlayerController? player, CommandInfo command, IList<Map> availibleMaps)
        {
            if (command == null)
                return;
            if (command.ArgCount == 1)
            {
                Responses.ReplyToPlayer(Statics.GetChatText("Available Maps:"), player);
                Responses.ReplyToPlayer(string.Join(", ", availibleMaps.Select(x => x.Name)), player);
                return;
            }
            var newMapName = command.GetArg(1);
            var newMap = availibleMaps.FirstOrDefault(map => map.Name == newMapName);
            if (newMap == null)
            {
                if (player != null)
                {
                    Responses.ReplyToPlayer($"Selected map {newMapName} not found. Available Maps:", player, true);
                    Responses.ReplyToPlayer(string.Join(", ", availibleMaps.Select(x => x.Name)), player);
                }
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

        public IList<Map> LoadMaps()
        {
            string fileName = "maps.json";
            string filePath = Path.Join(Statics.CfgPath, fileName);
            if (File.Exists(filePath))
            {
                List<Map> loadedMaps = new();
                try
                {
                    using (StreamReader fileReader = File.OpenText(filePath))
                    {
                        string jsonContent = fileReader.ReadToEnd();
                        if (!string.IsNullOrEmpty(jsonContent))
                        {
                            JsonSerializerOptions options = new()
                            {
                                AllowTrailingCommas = true,
                            };
                            loadedMaps = JsonSerializer.Deserialize<List<Map>>(jsonContent) ?? new List<Map>();
                        }
                    }
                    foreach (var map in loadedMaps)
                    {
                        Logging.Log($"[LoadMaps] Availible Maps: {map.Name}, Id: {map.Id}");
                    }
                    return loadedMaps;
                }
                catch (Exception e)
                {
                    Logging.Log($"[LoadMaps FATAL] An error occurred: {e.Message}");
                    return new List<Map>();
                }
            }
            else
            {

                List<Map> defaultMaps = new List<Map>
                {
                    new Map{Name = "de_ancient"},
                    new Map{Name = "cs_italy"},
                    new Map{Name = "cs_office"},
                    new Map{Name = "cs_vertigo"},
                    new Map{Name = "de_anubis"},
                    new Map{Name = "de_dust2"},
                    new Map{Name = "de_inferno"},
                    new Map{Name = "de_mirage"},
                    new Map{Name = "de_nuke"},
                    new Map{Name = "de_overpass"},
                    new Map{Name = "de_vertigo"}
                };
                try
                {
                    JsonSerializerOptions options = new()
                    {
                        WriteIndented = true,
                    };
                    string defaultJson = JsonSerializer.Serialize(defaultMaps, options);
                    string? directoryPath = Path.GetDirectoryName(filePath);
                    if (directoryPath != null)
                    {
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                    }
                    File.WriteAllText(filePath, defaultJson);

                    Logging.Log("[LoadMaps] Created a new JSON file with default content.");
                    return defaultMaps;
                }
                catch (Exception e)
                {
                    Logging.Log($"[LoadMaps FATAL] Error creating the JSON file: {e.Message}");
                    return new List<Map>();
                }
            }

        }
    }
}

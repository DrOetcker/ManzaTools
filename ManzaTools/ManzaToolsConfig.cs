using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace ManzaTools
{
    public class ManzaToolsConfig : BasePluginConfig
    {
        [JsonPropertyName("ConfigName")]
        public string ConfigName { get; set; } = "value";

        [JsonPropertyName("ConfigNameSecond")]
        public string ConfigNameSecond { get; set; } = "value";
    }
}
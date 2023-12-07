using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace ManzaTools.Config
{
    public class ManzaToolsConfig : BasePluginConfig
    {
        [JsonPropertyName("SmokeTimerEnabled")]
        public bool SmokeTimerEnabled { get; set; } = true;
    }
}
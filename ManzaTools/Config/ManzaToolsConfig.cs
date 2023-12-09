using CounterStrikeSharp.API.Core;
using ManzaTools.Models;

namespace ManzaTools.Config
{
    public class ManzaToolsConfig : BasePluginConfig
    {
        public GameModeEnum DefaultGameMode { get; set; } = GameModeEnum.Disabled;
        public bool SmokeTimerEnabled { get; set; } = true;
        public IList<Map> AvailibleMaps { get; set; } = new List<Map>();
    }
}
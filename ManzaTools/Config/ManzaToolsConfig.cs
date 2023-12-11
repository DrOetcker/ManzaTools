using CounterStrikeSharp.API.Core;

using ManzaTools.Models;

namespace ManzaTools.Config
{
    public class ManzaToolsConfig : BasePluginConfig
    {
        public virtual IList<Map> AvailableMaps { get; set; } = new List<Map>();

        public bool BlindTimerEnabled { get; set; } = true;

        public bool DamageReportEnabled { get; set; } = true;

        public GameModeEnum DefaultGameMode { get; set; } = GameModeEnum.Disabled;

        public bool SmokeTimerEnabled { get; set; } = true;
    }
}
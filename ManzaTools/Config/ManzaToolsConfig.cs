using CounterStrikeSharp.API.Core;
using DrOetcker.Models;

namespace ManzaTools.Config
{
    public class ManzaToolsConfig : BasePluginConfig
    {
        public bool SmokeTimerEnabled { get; set; } = true;
        public IList<Map> AvailibleMaps { get; set; } = new List<Map>();
    }
}
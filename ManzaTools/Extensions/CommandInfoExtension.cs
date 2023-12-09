using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Extensions
{
    public static class CommandInfoExtension
    {
        public static IList<string> ArgStringAsList(this CommandInfo commandInfo) {
            return commandInfo.ArgString.ToLower().Split(" ").ToList();
        }
    }
}

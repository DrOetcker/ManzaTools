using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces
{

    public interface IBaseService
    {
        public void AddCommands(Action<string, string, CommandInfo.CommandCallback> addCommand);
    }
}

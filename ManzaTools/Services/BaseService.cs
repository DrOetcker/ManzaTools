using CounterStrikeSharp.API.Modules.Commands;
using ManzaTools.Interfaces;
using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class BaseService : IBaseService
    {
        protected readonly ILogger _logger;

        public BaseService(ILogger logger)
        {
            _logger = logger;
        }

        public virtual void AddCommands(Action<string, string, CommandInfo.CommandCallback> addCommand) { }
    }
}

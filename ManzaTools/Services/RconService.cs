using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

using ManzaTools.Interfaces;
using ManzaTools.Utils;

using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class RconService : BaseService, IRconService
    {
        protected RconService(ILogger<RconService> logger)
            : base(logger)
        {
        }

        public void Execute(CCSPlayerController? player, CommandInfo info)
        {
            Server.ExecuteCommand(info.ArgString);
            Responses.ReplyToPlayer($"Command \"{info.ArgString}\" executed", player);
        }
    }
}

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
        public RconService(ILogger<RconService> logger)
            : base(logger)
        {
        }

        public override void Init(ManzaTools manzaTools)
        {
            manzaTools.AddCommand("css_rcon", "Executes a command on the server", Execute);
        }

        public void Execute(CCSPlayerController? player, CommandInfo info)
        {
            Server.ExecuteCommand(info.ArgString);
            if (player != null)
                Responses.ReplyToPlayer($"Command \"{info.ArgString}\" executed", player);
        }
    }
}

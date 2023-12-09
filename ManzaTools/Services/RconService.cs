using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using ManzaTools.Utils;

namespace ManzaTools.Services
{
    public class RconService
    {
        internal void Execute(CCSPlayerController? player, CommandInfo info)
        {
            Server.ExecuteCommand(info.ArgString);
            Responses.ReplyToPlayer($"Command \"{info.ArgString}\" executed", player);
        }
    }
}

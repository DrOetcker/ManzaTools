using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Services
{
    public class RethrowService
    {
        internal void Rethrow(CCSPlayerController? player, CommandInfo info)
        {
            Server.ExecuteCommand("sv_rethrow_last_grenade");
        }
    }
}

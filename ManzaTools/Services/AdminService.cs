using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using ManzaTools.Interfaces;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;

namespace ManzaTools.Services
{
    public class AdminService : PracticeBaseService, IAdminService
    {

        public AdminService(ILogger<AdminService> logger, IGameModeService gameModeService)
            : base(logger, gameModeService)
        {

        }

        public override void Init(ManzaTools manzaTools)
        {
            manzaTools.AddCommand("css_slap", "Places a bot with given params", Slap);
        }

        private void Slap(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (!AdminManager.PlayerHasPermissions(player, "@css/root"))
                return;

            if (commandInfo.ArgCount == 1)
                return;

            var playerName = commandInfo.GetArg(1);

            if (string.IsNullOrEmpty(playerName))
                return;

            SlapPlayer(playerName);
        }

        private void SlapPlayer(string playerName)
        {
            var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller").Where(x => x.IsValid && !x.IsBot && x.UserId.HasValue && !x.IsHLTV && x.SteamID > 0);
            foreach (var playerEntity in playerEntities)
            {
                if (playerEntity.PlayerName.ToLower() != playerName.ToLower())
                    continue;

                PerformSlap(playerEntity.PlayerPawn!.Value!, 5);
            }
        }

        private static void PerformSlap(CBasePlayerPawn pawn, int damage = 0)
        {
            if (pawn.LifeState != (int)LifeState_t.LIFE_ALIVE)
                return;

            var random = new Random();
            var vel = new Vector(pawn.AbsVelocity.X, pawn.AbsVelocity.Y, pawn.AbsVelocity.Z);

            vel.X += ((random.Next(180) + 50) * ((random.Next(2) == 1) ? -1 : 1));
            vel.Y += ((random.Next(180) + 50) * ((random.Next(2) == 1) ? -1 : 1));
            vel.Z += random.Next(200) + 100;

            pawn.AbsVelocity.X = vel.X;
            pawn.AbsVelocity.Y = vel.Y;
            pawn.AbsVelocity.Z = vel.Z;

            if (damage <= 0)
                return;

            pawn.Health -= damage;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");

            if (pawn.Health <= 0)
                pawn.CommitSuicide(true, true);
        }
    }
}

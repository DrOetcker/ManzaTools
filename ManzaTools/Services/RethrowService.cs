using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using ManzaTools.Interfaces;
using ManzaTools.Models;
using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class RethrowService : PracticeBaseService, IRethrowService
    {
        private IList<PersonalThrownGrenade> personalThrownGrenades = new List<PersonalThrownGrenade>();

        public RethrowService(ILogger<RethrowService> logger, IGameModeService gameModeService)
            : base(logger, gameModeService)
        {
        }

        public override void AddCommands(Action<string, string, CommandInfo.CommandCallback> addCommand)
        {
            addCommand("css_rethrow", "Rethrows the last thrown grenade on the Server", Rethrow);
            addCommand("css_last", "Positions the player on the last position where he threw a nade", Last);
        }

        public void Rethrow(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice)
                return;
            Server.ExecuteCommand("sv_rethrow_last_grenade");
        }

        public HookResult OnGrenadeThrown(EventGrenadeThrown @event, GameEventInfo info)
        {
            if (!GameModeIsPractice || !@event.Userid.IsValid || @event.Userid.IsBot)
                return HookResult.Continue;

            if (@event.Weapon != "smokegrenade" &&
                @event.Weapon != "molotov" &&
                @event.Weapon != "incgrenade" &&
                @event.Weapon != "flashbang" &&
                @event.Weapon != "hegrenade")
                return HookResult.Continue;

            var userThronGrenade = personalThrownGrenades.FirstOrDefault(x => x.UserSteamId == @event.Userid.SteamID);
            if (userThronGrenade != null)
                personalThrownGrenades.Remove(userThronGrenade);

            if (@event.Userid.PlayerPawn?.Value?.EyeAngles == null || @event.Userid.Pawn.Value?.CBodyComponent?.SceneNode?.AbsOrigin == null)
                return HookResult.Continue;

            var playerAngle = @event.Userid.PlayerPawn.Value.EyeAngles;
            var playerPos = @event.Userid.Pawn.Value.CBodyComponent.SceneNode.AbsOrigin;
            personalThrownGrenades.Add(new PersonalThrownGrenade
            {
                Weapon = @event.Weapon,
                UserSteamId = @event.Userid.SteamID,
                Position = new Vector(playerPos.X, playerPos.Y, playerPos.Z),
                Angle = new QAngle(playerAngle.X, playerAngle.Y, playerAngle.Z),
            });


            return HookResult.Continue;
        }

        public void Last(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player?.PlayerPawn.Value == null)
                return;

            var userThronGrenade = personalThrownGrenades.FirstOrDefault(x => x.UserSteamId == player.SteamID);
            if (userThronGrenade == null)
                return;

            player.PlayerPawn.Value.Teleport(userThronGrenade.Position, userThronGrenade.Angle, new Vector(0, 0, 0));
            player.GiveNamedItem($"weapon_{userThronGrenade.Weapon}");
            switch (userThronGrenade.Weapon)
            {
                case "flashbang":
                    player.ExecuteClientCommand("slot7");
                    break;
                case "smokegrenade":
                    player.ExecuteClientCommand("slot8");
                    break;
                case "hegrenade":
                    player.ExecuteClientCommand("slot6");
                    break;
                case "molotov":
                case "incgrenade":
                    player.ExecuteClientCommand("slot10");
                    break;
            }
        }
    }
}

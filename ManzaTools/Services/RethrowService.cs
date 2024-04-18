using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using ManzaTools.Interfaces;
using ManzaTools.Models;
using ManzaTools.Utils;
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

        public override void Init(ManzaTools manzaTools)
        {
            manzaTools.AddCommand("css_rethrow", "Rethrows the last thrown grenade on the Server", Rethrow);
            manzaTools.AddCommand("css_rethrowown", "Rethrows the last thrown grenade on the Server", RethrowOwn);
            manzaTools.AddCommand("css_last", "Positions the player on the last position where he threw a nade", Last);
            manzaTools.RegisterEventHandler<EventGrenadeThrown>(OnGrenadeThrown);
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

            var userThrownGrenade = personalThrownGrenades.FirstOrDefault(x => x.UserSteamId == @event.Userid.SteamID);
            if (userThrownGrenade != null)
                personalThrownGrenades.Remove(userThrownGrenade);

            if (@event.Userid.PlayerPawn?.Value?.EyeAngles == null || @event.Userid.Pawn.Value?.CBodyComponent?.SceneNode?.AbsOrigin == null)
                return HookResult.Continue;

            var playerAngle = @event.Userid.PlayerPawn.Value.EyeAngles;
            var playerPos = @event.Userid.Pawn.Value.CBodyComponent.SceneNode.AbsOrigin;


            CBaseCSGrenadeProjectile grenadeProjectile = null;
            var nadeType = String.Empty;
            switch (@event.Weapon)
            {
                case "smokegrenade":
                    {
                        grenadeProjectile = Utilities.FindAllEntitiesByDesignerName<CBaseCSGrenadeProjectile>("smokegrenade_projectile").First();
                        nadeType = Consts.Smoke;
                        break;
                    }
                case "flashbang":
                    {
                        grenadeProjectile = Utilities.FindAllEntitiesByDesignerName<CFlashbangProjectile>("flashbang_projectile").First();
                        nadeType = Consts.Flash;
                        break;
                    }
                case "hegrenade":
                    {
                        grenadeProjectile = Utilities.FindAllEntitiesByDesignerName<CHEGrenadeProjectile>("hegrenade_projectile").First();
                        nadeType = Consts.Nade;
                        break;
                    }
                case "molotov":
                case "incgrenade":
                    {
                        grenadeProjectile = Utilities.FindAllEntitiesByDesignerName<CHEGrenadeProjectile>("molotov_projectile").First();
                        nadeType = Consts.Molotov;
                        break;
                    }
                default:
                    break;
            }
            personalThrownGrenades.Add(new PersonalThrownGrenade
            {
                Weapon = nadeType,
                UserSteamId = @event.Userid.SteamID,
                PlayerPosition = new Vector(playerPos.X, playerPos.Y, playerPos.Z),
                PlayerAngle = new QAngle(playerAngle.X, playerAngle.Y, playerAngle.Z),
                Position = $"{grenadeProjectile.AbsOrigin!.X} {grenadeProjectile.AbsOrigin!.Y} {grenadeProjectile.AbsOrigin!.Z}",
                Velocity = $"{grenadeProjectile.AbsVelocity!.X} {grenadeProjectile.AbsVelocity!.Y} {grenadeProjectile.AbsVelocity!.Z}",
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

            player.PlayerPawn.Value.Teleport(userThronGrenade.PlayerPosition, userThronGrenade.PlayerAngle, new Vector(0, 0, 0));
            player.GiveNamedItem($"weapon_{userThronGrenade.Weapon}");
            switch (userThronGrenade.Weapon)
            {
                case Consts.Flash:
                    player.ExecuteClientCommand("slot7");
                    break;
                case Consts.Smoke:
                    player.ExecuteClientCommand("slot8");
                    break;
                case Consts.Nade:
                    player.ExecuteClientCommand("slot6");
                    break;
                case Consts.Molotov:
                case Consts.Inc:
                    player.ExecuteClientCommand("slot10");
                    break;
            }
        }

        public void RethrowOwn(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player?.PlayerPawn.Value == null)
                return;

            var userThrownGrenade = personalThrownGrenades.FirstOrDefault(x => x.UserSteamId == player.SteamID);
            if (userThrownGrenade == null)
                return;

            CBaseCSGrenadeProjectile? grenadeProjectile = null;
            switch (userThrownGrenade.Weapon)
            {
                case Consts.Flash:
                    grenadeProjectile = Utilities.CreateEntityByName<CFlashbangProjectile>("flashbang_projectile");
                    if (grenadeProjectile == null)
                    {
                        return;
                    }

                    grenadeProjectile.DispatchSpawn();
                    break;
                case Consts.Smoke:
                    var velocity = TeleportHelper.GetAngleFromJsonString(userThrownGrenade.Velocity);
                    grenadeProjectile = RecordService.CSmokeGrenadeProjectile_CreateFunc.Invoke(
                                    userThrownGrenade.PlayerPosition.Handle,
                                    new QAngle().Handle,
                                    velocity.Handle,
                                    velocity.Handle,
                                    IntPtr.Zero,
                                    45,
                                    1
                                );
                    break;
                case Consts.Nade:
                    grenadeProjectile = Utilities.CreateEntityByName<CHEGrenadeProjectile>("hegrenade_projectile");
                    if (grenadeProjectile == null)
                    {
                        return;
                    }

                    grenadeProjectile.Damage = 100;
                    grenadeProjectile.DmgRadius = grenadeProjectile.Damage * 3.5f;
                    grenadeProjectile.DispatchSpawn();
                    grenadeProjectile.AcceptInput("InitializeSpawnFromWorld");
                    break;
                case Consts.Molotov:
                case Consts.Inc:
                    grenadeProjectile = Utilities.CreateEntityByName<CMolotovProjectile>("molotov_projectile");
                    if (grenadeProjectile == null)
                    {
                        return;
                    }

                    grenadeProjectile.Damage = 200;
                    grenadeProjectile.DmgRadius = 300;

                    grenadeProjectile.DispatchSpawn();
                    grenadeProjectile.AcceptInput("InitializeSpawnFromWorld");
                    grenadeProjectile.SetModel("weapons/models/grenade/molotov/weapon_molotov.vmdl");
                    break;
            }

            if (grenadeProjectile != null && grenadeProjectile.DesignerName != "smokegrenade_projectile")
            {
                var position = TeleportHelper.GetVectorFromJsonString(userThrownGrenade.Position);
                var velocity = TeleportHelper.GetVectorFromJsonString(userThrownGrenade.Velocity);

                grenadeProjectile.InitialPosition.X = position.X;
                grenadeProjectile.InitialPosition.Y = position.Y;
                grenadeProjectile.InitialPosition.Z = position.Z;

                grenadeProjectile.InitialVelocity.X = velocity.X;
                grenadeProjectile.InitialVelocity.Y = velocity.Y;
                grenadeProjectile.InitialVelocity.Z = velocity.Z;

                grenadeProjectile.AngVelocity.X = velocity.X;
                grenadeProjectile.AngVelocity.Y = velocity.Y;
                grenadeProjectile.AngVelocity.Z = velocity.Z;

                grenadeProjectile.Teleport(position, new QAngle(), velocity);

                grenadeProjectile.TeamNum = 2;
            }
        }
    }
}

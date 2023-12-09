using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using ManzaTools.Extensions;
using ManzaTools.Models;
using ManzaTools.Utils;

namespace ManzaTools.Services
{
    public class BotService : PracticeBaseService
    {
        private IList<PlacedBots> currentPlacedBots = new List<PlacedBots>();
        private bool botSpawning = false;

        public BotService(GameModeService gameModeService)
            : base(gameModeService)
        {
        }

        internal void CreateBot(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player == null)
                return;
            botSpawning = true;
            var crouchBot = info.ArgStringAsList().Contains("crouch");
            byte teamSide = DetermineTeamSide(player.TeamNum, info.ArgStringAsList());

            AddBot(player, crouchBot, teamSide);
        }

        internal void RemoveBots(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player == null)
                return;

            var playerEntities = Utilities.GetPlayers().Where(x => x.IsValid && x.IsBot && x.UserId.HasValue && !x.IsHLTV);
            foreach (var playerEntity in playerEntities)
                Server.ExecuteCommand($"bot_kick {playerEntity.PlayerName}");
        }

        internal void RemoveBot(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player == null)
                return;

            var botToKick = info.ArgByIndex(1);

            var playerEntitiy = Utilities.GetPlayers().FirstOrDefault(x => x.IsValid && x.IsBot && x.UserId.HasValue && !x.IsHLTV && botToKick.ToLower() == x.PlayerName.ToLower());
            if (playerEntitiy == null)
            {
                player.PrintToChat($"Not bot named {botToKick} found.");
                return;
            }

            Server.ExecuteCommand($"bot_kick {playerEntitiy.PlayerName}");
        }

        private static byte DetermineTeamSide(byte playerTeamNum, IList<string> argList)
        {
            var isCounterTerrorist = argList.Contains("ct");
            var isTerrorist = argList.Contains("t");
            if (!isTerrorist && !isCounterTerrorist)
                return playerTeamNum;
            return isCounterTerrorist ? (byte)3 : (byte)2;
        }

        private void AddBot(CCSPlayerController player, bool crouchBot, byte teamNum)
        {
            if (PlayerExtension.IsCounterTerrorist(teamNum))
                Server.ExecuteCommand("bot_add_ct");
            else
                Server.ExecuteCommand("bot_add_t");

            Utils.Timer.CreateTimer(0.1f, () => HandleNewBot(player, crouchBot), null);
        }

        internal HookResult PositionBotOnRespawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            if (!GameModeIsPractice)
                return HookResult.Continue;
            var bot = @event.Userid;
            if (bot.IsValid && bot.IsBot && bot.UserId.HasValue && !bot.IsHLTV)
            {
                var placedBot = currentPlacedBots.Where(x => x.Bot.Index == bot.Index).FirstOrDefault();
                if (placedBot == null)
                {
                    if (!botSpawning)
                        Utils.Timer.CreateTimer(2.5f, () => Server.ExecuteCommand($"bot_kick {bot.PlayerName}"));
                    return HookResult.Continue;
                }

                TeleportBot(placedBot.Position, placedBot.Angle, bot, placedBot.Crouch);
            }
            return HookResult.Continue;
        }

        private void HandleNewBot(CCSPlayerController botOwner, bool crouchBot)
        {
            var playerEntities = Utilities.GetPlayers().Where(x => x.IsValid && x.IsBot && x.UserId.HasValue && !x.IsHLTV);
            var targetPosition = botOwner.PlayerPawn?.Value?.CBodyComponent?.SceneNode?.AbsOrigin;
            var targetViewAngle = botOwner.PlayerPawn?.Value?.CBodyComponent?.SceneNode?.AbsRotation;
            if (targetPosition == null || targetViewAngle == null)
                return;

            foreach (var playerEntity in playerEntities)
            {
                var alredyExistingBot = currentPlacedBots.FirstOrDefault(x => x.Bot.Index == playerEntity.Index);
                if (alredyExistingBot != null)
                    continue;
                TeleportBot(targetPosition, targetViewAngle, playerEntity, crouchBot);
                currentPlacedBots.Add(new PlacedBots
                {
                    Position = new Vector(targetPosition.X, targetPosition.Y, targetPosition.Z),
                    Angle = new QAngle(targetViewAngle.X, targetViewAngle.Y, targetViewAngle.Z),
                    Crouch = crouchBot,
                    PlayerName = playerEntity.PlayerName,
                    Owner = botOwner,
                    Bot = playerEntity
                });
                TemporarilyDisableCollisions(botOwner, playerEntity);
            }

        }

        private void TeleportBot(Vector targetPosition, QAngle targetViewAngle, CCSPlayerController playerEntity, bool crouchBot)
        {
            playerEntity.PlayerPawn.Value.Teleport(targetPosition, targetViewAngle, new Vector(0, 0, 0));
            playerEntity.PlayerPawn.Value.Flags |= 2;
            CCSPlayer_MovementServices movementService = new(playerEntity.PlayerPawn.Value.MovementServices!.Handle);
            Utils.Timer.CreateTimer(0.1f, () => movementService.DuckAmount = 1);
            Utils.Timer.CreateTimer(0.2f, () => playerEntity.PlayerPawn.Value.Bot.IsCrouching = crouchBot);
            botSpawning = false;
        }

        private CounterStrikeSharp.API.Modules.Timers.Timer? collisionGroupTimer;
        //Creds: https://github.com/shobhit-pathak/MatchZy/blob/d6f7d47998d01a739e22618f7016b1d73ada870f/PracticeMode.cs#L778
        public void TemporarilyDisableCollisions(CCSPlayerController p1, CCSPlayerController p2)
        {
            p1.PlayerPawn.Value.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEBRIS;
            p1.PlayerPawn.Value.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEBRIS;
            p2.PlayerPawn.Value.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEBRIS;
            p2.PlayerPawn.Value.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEBRIS;
            var p1p = p1.PlayerPawn;
            var p2p = p2.PlayerPawn;
            collisionGroupTimer?.Kill();
            collisionGroupTimer = Utils.Timer.CreateTimer(0.1f, () =>
            {
                if (!p1p.IsValid || !p2p.IsValid || !p1p.Value.IsValid || !p2p.Value.IsValid)
                {
                    Logging.Log($"player handle invalid p1p {p1p.Value.IsValid} p2p {p2p.Value.IsValid}");
                    collisionGroupTimer?.Kill();
                    return;
                }

                if (!DoPlayersCollide(p1p.Value, p2p.Value))
                {
                    // Once they no longer collide 
                    p1p.Value.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_PLAYER_MOVEMENT;
                    p1p.Value.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_PLAYER_MOVEMENT;
                    p2p.Value.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_PLAYER_MOVEMENT;
                    p2p.Value.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_PLAYER_MOVEMENT;
                    collisionGroupTimer?.Kill();
                }

            }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
        }

        public bool DoPlayersCollide(CCSPlayerPawn p1, CCSPlayerPawn p2)
        {
            Vector p1min, p1max, p2min, p2max;
            var p1pos = p1.AbsOrigin;
            var p2pos = p2.AbsOrigin;
            p1min = p1.Collision.Mins + p1pos!;
            p1max = p1.Collision.Maxs + p1pos!;
            p2min = p2.Collision.Mins + p2pos!;
            p2max = p2.Collision.Maxs + p2pos!;

            return p1min.X <= p2max.X && p1max.X >= p2min.X &&
                    p1min.Y <= p2max.Y && p1max.Y >= p2min.Y &&
                    p1min.Z <= p2max.Z && p1max.Z >= p2min.Z;
        }
    }
}

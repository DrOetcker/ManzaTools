using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;

using ManzaTools.Extensions;
using ManzaTools.Interfaces;
using ManzaTools.Models;
using ManzaTools.Utils;

using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class BotService : PracticeBaseService, IBotService
    {
        private bool botSpawning;

        private CounterStrikeSharp.API.Modules.Timers.Timer? collisionGroupTimer;
        private readonly IList<PlacedBots> currentPlacedBots = new List<PlacedBots>();

        protected BotService(ILogger<BotService> logger, IGameModeService gameModeService)
            : base(logger, gameModeService)
        {
        }

        private void AddBot(CCSPlayerController player, bool crouchBot, byte teamNum)
        {
            if (PlayerExtension.IsCounterTerrorist(teamNum))
                Server.ExecuteCommand("bot_add_ct");
            else
                Server.ExecuteCommand("bot_add_t");

            Utils.Timer.CreateTimer(0.1f, () => HandleNewBot(player, crouchBot));
        }

        public void CreateBot(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player == null)
                return;
            botSpawning = true;
            var crouchBot = info.ArgStringAsList().Contains("crouch");
            var teamSide = DetermineTeamSide(player.TeamNum, info.ArgStringAsList());

            AddBot(player, crouchBot, teamSide);
        }

        private static byte DetermineTeamSide(byte playerTeamNum, IList<string> argList)
        {
            byte? ctNum = argList.Contains("ct") ? PlayerExtension.TeamToTeamNum("ct") : null;
            byte? tNum = argList.Contains("t") ? PlayerExtension.TeamToTeamNum("t") : null;

            return ctNum ?? tNum ?? (PlayerExtension.IsCounterTerrorist(playerTeamNum) ? (byte)2 : (byte)3);
        }

        private bool DoPlayersCollide(CCSPlayerPawn botOwnerPlayerPawn, CCSPlayerPawn botPlayerPawn)
        {
            Vector p1min, p1max, p2min, p2max;
            var p1pos = botOwnerPlayerPawn.AbsOrigin;
            var p2pos = botPlayerPawn.AbsOrigin;
            p1min = botOwnerPlayerPawn.Collision.Mins + p1pos!;
            p1max = botOwnerPlayerPawn.Collision.Maxs + p1pos!;
            p2min = botPlayerPawn.Collision.Mins + p2pos!;
            p2max = botPlayerPawn.Collision.Maxs + p2pos!;

            return p1min.X <= p2max.X && p1max.X >= p2min.X &&
                    p1min.Y <= p2max.Y && p1max.Y >= p2min.Y &&
                    p1min.Z <= p2max.Z && p1max.Z >= p2min.Z;
        }

        private void HandleNewBot(CCSPlayerController botOwner, bool crouchBot)
        {
            var playerEntities = Utilities.GetPlayers().Where(x => x.IsValid && x.IsBot && x.UserId.HasValue && !x.IsHLTV);
            var targetPosition = botOwner.PlayerPawn.Value?.CBodyComponent?.SceneNode?.AbsOrigin;
            var targetViewAngle = botOwner.PlayerPawn.Value?.CBodyComponent?.SceneNode?.AbsRotation;
            if (targetPosition == null || targetViewAngle == null)
                return;

            foreach (var playerEntity in playerEntities)
            {
                var alreadyExistingBot = currentPlacedBots.FirstOrDefault(x => x.Bot.Index == playerEntity.Index);
                if (alreadyExistingBot != null)
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

        public HookResult PositionBotOnRespawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            if (!GameModeIsPractice)
                return HookResult.Continue;
            var bot = @event.Userid;
            if (bot.IsValid && bot.IsBot && bot.UserId.HasValue && !bot.IsHLTV)
            {
                var placedBot = currentPlacedBots.FirstOrDefault(x => x.Bot.Index == bot.Index);
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

        public void RemoveBot(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player == null)
                return;

            var botToKick = info.ArgByIndex(1);

            var playerEntity = Utilities.GetPlayers().FirstOrDefault(x => x.IsValid && x.IsBot && x.UserId.HasValue && !x.IsHLTV && botToKick.ToLower() == x.PlayerName.ToLower());
            if (playerEntity == null)
            {
                player.PrintToChat($"Not bot named \"{botToKick}\" found.");
                return;
            }

            Server.ExecuteCommand($"bot_kick {playerEntity.PlayerName}");
        }

        public void RemoveBots(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player == null)
                return;

            var playerEntities = Utilities.GetPlayers().Where(x => x.IsValid && x.IsBot && x.UserId.HasValue && !x.IsHLTV);
            foreach (var playerEntity in playerEntities)
                Server.ExecuteCommand($"bot_kick {playerEntity.PlayerName}");
        }

        private void TeleportBot(Vector targetPosition, QAngle targetViewAngle, CCSPlayerController playerEntity, bool crouchBot)
        {
            if (playerEntity.PlayerPawn.Value?.Bot == null)
                return;
            playerEntity.PlayerPawn.Value.Teleport(targetPosition, targetViewAngle, new Vector(0, 0, 0));
            playerEntity.PlayerPawn.Value.Flags |= 2;
            CCSPlayer_MovementServices movementService = new(playerEntity.PlayerPawn.Value.MovementServices!.Handle);
            Utils.Timer.CreateTimer(0.1f, () => movementService.DuckAmount = 1);
            Utils.Timer.CreateTimer(0.2f, () => playerEntity.PlayerPawn.Value.Bot.IsCrouching = crouchBot);
            botSpawning = false;
        }

        //Creds: https://github.com/shobhit-pathak/MatchZy/blob/d6f7d47998d01a739e22618f7016b1d73ada870f/PracticeMode.cs#L778
        private void TemporarilyDisableCollisions(CCSPlayerController botOwner, CCSPlayerController bot)
        {
            if (botOwner.PlayerPawn.Value == null || bot.PlayerPawn.Value == null)
            {
                this._logger.LogError("Could not perform disable Collision");
                return;
            }
            botOwner.PlayerPawn.Value.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEBRIS;
            botOwner.PlayerPawn.Value.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEBRIS;
            bot.PlayerPawn.Value.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEBRIS;
            bot.PlayerPawn.Value.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEBRIS;
            var botOwnerPlayerPawn = botOwner.PlayerPawn;
            var botPlayerPawn = bot.PlayerPawn;
            collisionGroupTimer?.Kill();
            collisionGroupTimer = Utils.Timer.CreateTimer(0.1f, () =>
            {
                if (!botOwnerPlayerPawn.IsValid || !botPlayerPawn.IsValid || !botOwnerPlayerPawn.Value.IsValid || !botPlayerPawn.Value.IsValid)
                {
                    Logging.Log($"player handle invalid p1p {botOwnerPlayerPawn.Value.IsValid} p2p {botPlayerPawn.Value.IsValid}");
                    collisionGroupTimer?.Kill();
                    return;
                }

                if (!DoPlayersCollide(botOwnerPlayerPawn.Value, botPlayerPawn.Value))
                {
                    // Once they no longer collide 
                    botOwnerPlayerPawn.Value.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_PLAYER_MOVEMENT;
                    botOwnerPlayerPawn.Value.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_PLAYER_MOVEMENT;
                    botPlayerPawn.Value.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_PLAYER_MOVEMENT;
                    botPlayerPawn.Value.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_PLAYER_MOVEMENT;
                    collisionGroupTimer?.Kill();
                }

            }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
        }
    }
}

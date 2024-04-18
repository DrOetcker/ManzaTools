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

        public BotService(ILogger<BotService> logger, IGameModeService gameModeService)
            : base(logger, gameModeService)
        {
        }

        public override void Init(ManzaTools manzaTools)
        {
            manzaTools.RegisterListener((Listeners.OnMapStart)(entity => currentPlacedBots?.Clear()));
            manzaTools.RegisterEventHandler<EventPlayerSpawn>((@event, info) => PositionBotOnRespawn(@event, info));
            manzaTools.AddCommand("css_bot", "Places a bot with given params", CreateBot);
            manzaTools.AddCommand("css_bot_kick", "Removes a single bots", RemoveBot);
            manzaTools.AddCommand("css_bots_kick", "Removes all bots", RemoveBots);
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
            var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller").Where(x => x.IsValid && x.IsBot && x.UserId.HasValue && !x.IsHLTV);
            var targetPosition = botOwner.PlayerPawn.Value?.CBodyComponent?.SceneNode?.AbsOrigin;
            var targetViewAngle = botOwner.PlayerPawn.Value?.CBodyComponent?.SceneNode?.AbsRotation;
            if (targetPosition == null || targetViewAngle == null)
            {
                Responses.SendDebug($"No bot position found: {targetPosition} {targetViewAngle}", nameof(BotService), nameof(HandleNewBot));
                return;
            }

            var firstPlaceBots = playerEntities.Where(x => currentPlacedBots.All(y => y.Bot.Index != x.Index));
            foreach (var firstPlaceBot in firstPlaceBots)
            {
                var alreadyExistingBot = currentPlacedBots.FirstOrDefault(x => x.Bot.Index == firstPlaceBot.Index);
                if (alreadyExistingBot != null)
                {
                    Responses.SendDebug($"Bot to place already exists", nameof(BotService), nameof(HandleNewBot));
                    continue;
                }
                TeleportBot(targetPosition, targetViewAngle, firstPlaceBot, crouchBot);
                currentPlacedBots.Add(new PlacedBots
                {
                    Position = new Vector(targetPosition.X, targetPosition.Y, targetPosition.Z),
                    Angle = new QAngle(targetViewAngle.X, targetViewAngle.Y, targetViewAngle.Z),
                    Crouch = crouchBot,
                    PlayerName = firstPlaceBot.PlayerName,
                    Owner = botOwner,
                    Bot = firstPlaceBot
                });
                TemporarilyDisableCollisions(botOwner, firstPlaceBot);
            }

        }

        public HookResult PositionBotOnRespawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            if (!GameModeIsPractice || !IsPlayerValid(@event.Userid))
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
            else
            {
                Responses.SendDebug($"PlayerEntity to position is not valid: {bot.IsValid} {bot.IsBot} {bot.UserId.HasValue} {!bot.IsHLTV}",nameof(BotService), nameof(PositionBotOnRespawn));
            }
            return HookResult.Continue;
        }

        public void RemoveBot(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player == null)
                return;

            var botToKick = info.ArgByIndex(1);

            var playerEntity = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller").FirstOrDefault(x => x.IsValid && x.IsBot && x.UserId.HasValue && !x.IsHLTV && botToKick.ToLower() == x.PlayerName.ToLower());
            if (playerEntity == null)
            {
                player.PrintToChat($"Not bot named \"{botToKick}\" found.");
                return;
            }
            currentPlacedBots.Remove(currentPlacedBots.First(x => x.PlayerName?.ToLower() == playerEntity.PlayerName.ToLower()));
            Server.ExecuteCommand($"bot_kick {playerEntity.PlayerName}");
        }

        public void RemoveBots(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player == null)
                return;

            var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller").Where(x => x.IsValid && x.IsBot && x.UserId.HasValue && !x.IsHLTV);
            foreach (var playerEntity in playerEntities)
            {
                currentPlacedBots.Remove(currentPlacedBots.First(x => x.PlayerName?.ToLower() == playerEntity.PlayerName.ToLower()));
                Server.ExecuteCommand($"bot_kick {playerEntity.PlayerName}");
            }
        }

        private void TeleportBot(Vector targetPosition, QAngle targetViewAngle, CCSPlayerController playerEntity, bool crouchBot)
        {
            try
            {
                if (!playerEntity.PlayerPawn.IsValid || playerEntity.PlayerPawn.Value?.IsValid == false || playerEntity.PlayerPawn.Value?.Bot == null)
                {
                    Responses.SendDebug($"Bot to teleport is not valid: {!playerEntity.PlayerPawn.IsValid} {playerEntity.PlayerPawn.Value?.Bot == null}", nameof(BotService), nameof(TeleportBot));
                    return;
                }
                playerEntity.PlayerPawn.Value.Teleport(targetPosition, targetViewAngle, new Vector(0, 0, 0));
                playerEntity.PlayerPawn.Value.Flags |= 2;
                CCSPlayer_MovementServices movementService = new(playerEntity.PlayerPawn.Value.MovementServices!.Handle);
                Utils.Timer.CreateTimer(0.1f, () => movementService.DuckAmount = 1);
                Utils.Timer.CreateTimer(0.2f, () => playerEntity.PlayerPawn.Value.Bot.IsCrouching = crouchBot);
                botSpawning = false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not teleport Bot", ex);
            }
        }

        //Creds: https://github.com/shobhit-pathak/MatchZy/blob/d6f7d47998d01a739e22618f7016b1d73ada870f/PracticeMode.cs#L778
        private void TemporarilyDisableCollisions(CCSPlayerController botOwner, CCSPlayerController bot)
        {
            if (botOwner.PlayerPawn.Value == null || bot.PlayerPawn.Value == null)
            {
                _logger.LogError("Could not perform disable Collision");
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
                    _logger.LogError($"player handle invalid p1p {botOwnerPlayerPawn.Value.IsValid} p2p {botPlayerPawn.Value.IsValid}");
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

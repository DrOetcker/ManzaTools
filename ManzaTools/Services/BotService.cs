using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
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

        public BotService(GameModeService gameModeService)
            : base(gameModeService)
        {
        }

        internal void CreateBot(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player == null)
                return;

            AddBot(player);
        }

        internal HookResult PositionBotOnRespawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            if (!GameModeIsPractice)
                return HookResult.Continue;
            var bot = @event.Userid;
            if (!bot.IsBot || bot.IsHLTV)
                return HookResult.Continue;

            var placedBot = currentPlacedBots.Where(x => x.Bot.Index == bot.Index).FirstOrDefault();
            if (placedBot == null)
                return HookResult.Continue;

            SetBotPosition(placedBot.Position, placedBot.Angle, bot);


            return HookResult.Continue;
        }

        private void AddBot(CCSPlayerController player)
        {
            if (player.IsCounterTerrorist())
                Server.ExecuteCommand("bot_add_t");
            else
                Server.ExecuteCommand("bot_add_ct");

            Utils.Timer.CreateTimer(0.1f, () => MoveBotToPlayer(player), null);
        }

        private void MoveBotToPlayer(CCSPlayerController botOwner)
        {
            var playerEntities = Utilities.GetPlayers().Where(x => x.IsBot && !x.IsHLTV);
            var targetPosition = botOwner.PlayerPawn?.Value?.CBodyComponent?.SceneNode?.AbsOrigin;
            var targetViewAngle = botOwner.PlayerPawn?.Value?.CBodyComponent?.SceneNode?.AbsRotation;
            if (targetPosition == null || targetViewAngle == null)
                return;

            foreach (var playerEntity in playerEntities)
            {
                var alredyExistingBot = currentPlacedBots.FirstOrDefault(x => x.Bot.Index == playerEntity.Index);
                if (alredyExistingBot != null)
                    continue;
                SetBotPosition(targetPosition, targetViewAngle, playerEntity);
                currentPlacedBots.Add(new PlacedBots
                {
                    Position = new Vector(targetPosition.X, targetPosition.Y, targetPosition.Z),
                    Angle = new QAngle(targetViewAngle.X, targetViewAngle.Y, targetViewAngle.Z),
                    Crouch = false,
                    PlayerName = playerEntity.PlayerName,
                    Owner = botOwner,
                    Bot = playerEntity
                });
                TemporarilyDisableCollisions(botOwner, playerEntity);
            }

        }

        private static void SetBotPosition(Vector targetPosition, QAngle targetViewAngle, CCSPlayerController playerEntity)
        {
            playerEntity.PlayerPawn.Value.Teleport(targetPosition, targetViewAngle, new Vector(0, 0, 0));
        }

        //Creds: https://github.com/shobhit-pathak/MatchZy/blob/d6f7d47998d01a739e22618f7016b1d73ada870f/PracticeMode.cs#L778
        private CounterStrikeSharp.API.Modules.Timers.Timer? collisionGroupTimer;
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

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using ManzaTools.Interfaces;
using ManzaTools.Models;
using ManzaTools.Utils;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace ManzaTools.Services
{
    public class RecordService : PracticeBaseService, IRecordService
    {
        private static readonly string savedReplaysFilePath = Path.Join(Statics.CfgPath, "savedReplays.json");
        private bool recording = false;
        private Replay currentReplay;
        private Stopwatch stopWatch = new Stopwatch();

        public RecordService(ILogger<RecordService> logger, IGameModeService gameModeService)
            : base(logger, gameModeService)
        {
        }

        public override void Init(ManzaTools manzaTools)
        {
            manzaTools.AddCommand("css_rec", "starts a recording", StartRecording);
            manzaTools.AddCommand("css_recstop", "starts a recording", StopRecording);
            manzaTools.AddCommand("css_testreplay", "starts a recording", TestRecorededReplay);
            manzaTools.AddCommand("css_savereplay", "starts a recording", SaveRecordedReplay);
            manzaTools.RegisterEventHandler<EventGrenadeThrown>(OnGrenadeThrown);
        }

        public HookResult OnGrenadeThrown(EventGrenadeThrown @event, GameEventInfo info)
        {
            if (!GameModeIsPractice || !recording || !@event.Userid.IsValid)
                return HookResult.Continue;

            if (currentReplay == null)
            {
                Responses.ReplyToServer("No running recording found");
                return HookResult.Continue;
            }

            CBaseCSGrenadeProjectile grenadeProjectile = null;
            switch (@event.Weapon)
            {
                case "smokegrenade":
                    {
                        grenadeProjectile = Utilities.FindAllEntitiesByDesignerName<CBaseCSGrenadeProjectile>("smokegrenade_projectile").First();
                        Responses.ReplyToServer($"{@event.Weapon}");
                        break;
                    }
                case "flashbang":
                    {
                        grenadeProjectile = Utilities.FindAllEntitiesByDesignerName<CFlashbangProjectile>("flashbang_projectile").First();
                        Responses.ReplyToServer($"{@event.Weapon}");
                        break;
                    }
                case "hegrenade":
                    {
                        grenadeProjectile = Utilities.FindAllEntitiesByDesignerName<CHEGrenadeProjectile>("hegrenade_projectile").First();
                        Responses.ReplyToServer($"{@event.Weapon}");
                        break;
                    }
                case "molotov":
                case "incgrenade":
                    {
                        grenadeProjectile = Utilities.FindAllEntitiesByDesignerName<CHEGrenadeProjectile>("molotov_projectile").First();
                        Responses.ReplyToServer($"{@event.Weapon}");
                        break;
                    }
                default:
                    break;
            }
            if (grenadeProjectile != null)
            {
                currentReplay.RecordedNades.Add(new RecordedNade
                {
                    Type = @event.Weapon,
                    Position = $"{grenadeProjectile.AbsOrigin!.X} {grenadeProjectile.AbsOrigin!.Y} {grenadeProjectile.AbsOrigin!.Z}",
                    Angle = $"{grenadeProjectile.AbsRotation!.X} {grenadeProjectile.AbsRotation!.Y} {grenadeProjectile.AbsRotation!.Z}",
                    Velocity = $"{grenadeProjectile.AbsVelocity!.X} {grenadeProjectile.AbsVelocity!.Y} {grenadeProjectile.AbsVelocity!.Z}",
                    ThrownAt = stopWatch.Elapsed
                });
            }

            return HookResult.Continue;
        }

        private void StartRecording(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (!GameModeIsPractice || recording || player == null)
                return;
            Responses.ReplyToPlayer("Start Recording", player);
            recording = true;
            stopWatch.Restart();

            currentReplay = new Replay(player.SteamID, DateTime.UtcNow);
        }

        private void StopRecording(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (!GameModeIsPractice || !recording || player == null)
                return;
            recording = false;

            if (currentReplay == null)
                return;
            currentReplay.Finished = true;
            stopWatch.Stop();
            currentReplay.Duration = stopWatch.Elapsed;
            Responses.ReplyToPlayer("Recording finished. Test with !testreplay", player);
        }

        private void TestRecorededReplay(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (!GameModeIsPractice || recording || player == null || currentReplay == null)
                return;
            PlayReplay(currentReplay);
        }

        private void SaveRecordedReplay(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (!GameModeIsPractice || recording || player == null || currentReplay == null)
                return;

            if (commandInfo.ArgCount == 1)
            {
                Responses.ReplyToPlayer("Usage: !savereplay \"My replayname\"", player, true);
                return;
            }
            var replayName = commandInfo.ArgByIndex(1);
            var description = commandInfo.ArgByIndex(2);
            var playerName = player.PlayerName;
            var playerSteamId = player.SteamID.ToString();
            var mapName = Server.MapName;

            VerifySavedReplaysFileExists();

            var savedReplays = GetExistingReplays();

            try
            {
                currentReplay.Id = GetIndexForNewReplay(savedReplays);
                currentReplay.Map = mapName;
                currentReplay.Creator = playerName;
                currentReplay.CreatorSteamId = playerSteamId;
                currentReplay.Name = replayName;
                currentReplay.Description = description;
                savedReplays.Add(currentReplay);

                File.WriteAllText(savedReplaysFilePath, JsonSerializer.Serialize(savedReplays));
                Responses.ReplyToPlayer($"Replay saved: Id: {currentReplay.Id}, {currentReplay.Name}, {currentReplay.Description}", player);
                currentReplay = null;
            }
            catch (JsonException ex)
            {
                _logger.LogError("Could not save nade", ex);
            }
        }

        private static uint GetIndexForNewReplay(IEnumerable<Replay> savedReplays)
        {
            if (!savedReplays.Any())
                return 1;
            var lastSavedReplayId = savedReplays.Max(x => x.Id);
            return lastSavedReplayId + 1;
        }

        private void PlayReplay(Replay recordToReplay)
        {
            Responses.ReplyToServer($"Count Nades: {recordToReplay.RecordedNades.Count}");
            foreach (var item in recordToReplay.RecordedNades)
            {
                new CounterStrikeSharp.API.Modules.Timers.Timer((float)(item.ThrownAt.TotalMilliseconds / 1000), () => ThrownGrenade(item), TimerFlags.STOP_ON_MAPCHANGE);
            }
        }

        private void ThrownGrenade(RecordedNade item)
        {
            CBaseCSGrenadeProjectile? grenadeProjectile = null;
            switch (item.Type)
            {
                case "smokegrenade":
                    Responses.ReplyToServer($"replay: {item.Type}");
                    var position = GetVector(item.Position);
                    var angle = GetAngle(item.Angle);
                    var velocity = GetAngle(item.Velocity);
                    var thrownSmoke = CSmokeGrenadeProjectile_CreateFunc.Invoke(
                                    position.Handle,
                                    angle.Handle,
                                    velocity.Handle,
                                    velocity.Handle,
                                    IntPtr.Zero,
                                    45,
                                    1
                                );
                    break;
                case "flashbang":
                    Responses.ReplyToServer($"replay: {item.Type}");
                    grenadeProjectile = Utilities.CreateEntityByName<CFlashbangProjectile>("flashbang_projectile");
                    if (grenadeProjectile == null)
                    {
                        return;
                    }

                    grenadeProjectile.DispatchSpawn();
                    break;
                case "hegrenade":
                    Responses.ReplyToServer($"replay: {item.Type}");
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
                case "molotov":
                case "incgrenade":
                    Responses.ReplyToServer($"replay: {item.Type}");
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
                var position = GetVector(item.Position);
                var angle = GetAngle(item.Angle);
                var velocity = GetVector(item.Velocity);
                grenadeProjectile.Teleport(position, angle, velocity);

                grenadeProjectile.InitialPosition.X = position.X;
                grenadeProjectile.InitialPosition.Y = position.Y;
                grenadeProjectile.InitialPosition.Z = position.Z;

                grenadeProjectile.InitialVelocity.X = velocity.X;
                grenadeProjectile.InitialVelocity.Y = velocity.Y;
                grenadeProjectile.InitialVelocity.Z = velocity.Z;

                grenadeProjectile.AngVelocity.X = velocity.X;
                grenadeProjectile.AngVelocity.Y = velocity.Y;
                grenadeProjectile.AngVelocity.Z = velocity.Z;

                grenadeProjectile.TeamNum = 1;
            }
        }

        private static MemoryFunctionWithReturn<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int, CSmokeGrenadeProjectile> CSmokeGrenadeProjectile_CreateFunc = new(
                Environment.OSVersion.Platform == PlatformID.Unix
                    ? @"\x55\x4C\x89\xC1\x48\x89\xE5\x41\x57\x41\x56\x49\x89\xD6"
                    : @"\x48\x89\x5C\x24\x2A\x48\x89\x6C\x24\x2A\x48\x89\x74\x24\x2A\x57\x41\x56\x41\x57\x48\x83\xEC\x50\x4C\x8B\xB4\x24");

        #region LoadSaveFileManagement

        private static IList<Replay> GetExistingReplays(string? mapName = null)
        {
            if (!File.Exists(savedReplaysFilePath))
                return new List<Replay>();

            var existingRecordingsJson = File.ReadAllText(savedReplaysFilePath);
            var savedRecordings = JsonSerializer.Deserialize<IList<Replay>>(existingRecordingsJson) ?? new List<Replay>();
            return string.IsNullOrEmpty(mapName) ?
                       savedRecordings :
                       savedRecordings.Where(x => x.Map == mapName).ToList();
        }

        private static void VerifySavedReplaysFileExists()
        {
            if (!File.Exists(savedReplaysFilePath))
                File.WriteAllText(savedReplaysFilePath, "[]");
        }

        private static Vector GetVector(string playerPosition)
        {
            var coordinates = playerPosition.Split(' ');
            return new Vector(float.Parse(coordinates[0]), float.Parse(coordinates[1]), float.Parse(coordinates[2]));
        }

        private static QAngle GetAngle(string playerAngle)
        {
            var coordinates = playerAngle.Split(' ');
            return new QAngle(float.Parse(coordinates[0]), float.Parse(coordinates[1]), float.Parse(coordinates[2]));
        }
        #endregion
    }
}

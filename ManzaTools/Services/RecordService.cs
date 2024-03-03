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
using System.Runtime.CompilerServices;

namespace ManzaTools.Services
{
    public class RecordService : PracticeBaseService, IRecordService
    {
        private bool recording = false;
        private IList<Recording> currentRecordings = new List<Recording>();
        private Stopwatch stopWatch = new Stopwatch();

        public RecordService(ILogger<RecordService> logger, IGameModeService gameModeService)
            : base(logger, gameModeService)
        {
        }

        public override void Init(ManzaTools manzaTools)
        {
            manzaTools.AddCommand("css_rec", "starts a recording", StartRecording);
            manzaTools.AddCommand("css_recstop", "starts a recording", StopRecording);
            manzaTools.AddCommand("css_replay", "starts a recording", Replay);
            manzaTools.RegisterEventHandler<EventGrenadeThrown>(OnGrenadeThrown);
        }

        public HookResult OnGrenadeThrown(EventGrenadeThrown @event, GameEventInfo info)
        {
            if (!GameModeIsPractice || !recording || !@event.Userid.IsValid)
                return HookResult.Continue;

            var currentRecording = currentRecordings.FirstOrDefault(x => x.SteamId == @event.Userid.SteamID && !x.Finished);
            if (currentRecording == null)
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
                currentRecordings.Last().RecordedNades.Add(new RecordedNade
                {
                    Type = @event.Weapon,
                    Position = new Vector(grenadeProjectile.AbsOrigin!.X, grenadeProjectile.AbsOrigin!.Y, grenadeProjectile.AbsOrigin!.Z),
                    Angle = new QAngle(grenadeProjectile.AbsRotation!.X, grenadeProjectile.AbsRotation!.Y, grenadeProjectile.AbsRotation!.Z),
                    Velocity = new Vector(grenadeProjectile.AbsVelocity!.X, grenadeProjectile.AbsVelocity!.Y, grenadeProjectile.AbsVelocity!.Z),
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

            currentRecordings.Add(new Recording(player.SteamID, currentRecordings.Count + 1, DateTime.UtcNow));
        }

        private void StopRecording(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (!GameModeIsPractice || !recording || player == null)
                return;
            Responses.ReplyToPlayer("Stop Recording", player);
            recording = false;

            var currentRecording = currentRecordings.FirstOrDefault(x => x.SteamId == player.SteamID && !x.Finished);
            if (currentRecording == null)
                return;
            currentRecording.Finished = true;
            stopWatch.Stop();
            currentRecording.Duration = stopWatch.Elapsed;
            Responses.ReplyToPlayer("Recording saved", player);
        }

        private void Replay(CCSPlayerController? player, CommandInfo commandInfo)
        {

            try
            {
                PlayReplay(currentRecordings.Last());

            }
            catch (Exception ex)
            {
                Responses.ReplyToPlayer("fück", player);
                _logger.LogError("Error on create & Fire Event", ex);
            }
        }

        private async Task PlayReplay(Recording recordToReplay)
        {
            Responses.ReplyToServer($"Count Nades: {recordToReplay.RecordedNades.Count}");
            var elapsedTimeInCurrentReplay = new TimeSpan();
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
                    var thrownSmoke = CSmokeGrenadeProjectile_CreateFunc.Invoke(
                                    item.Position.Handle,
                                    item.Angle.Handle,
                                    item.Velocity.Handle,
                                    item.Velocity.Handle,
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
                grenadeProjectile.Teleport(item.Position, item.Angle, item.Velocity);

                grenadeProjectile.InitialPosition.X = item.Position!.X;
                grenadeProjectile.InitialPosition.Y = item.Position!.Y;
                grenadeProjectile.InitialPosition.Z = item.Position!.Z;

                grenadeProjectile.InitialVelocity.X = item.Velocity!.X;
                grenadeProjectile.InitialVelocity.Y = item.Velocity!.Y;
                grenadeProjectile.InitialVelocity.Z = item.Velocity!.Z;

                grenadeProjectile.AngVelocity.X = item.Velocity!.X;
                grenadeProjectile.AngVelocity.Y = item.Velocity!.Y;
                grenadeProjectile.AngVelocity.Z = item.Velocity!.Z;

                grenadeProjectile.TeamNum = 1;
            }
        }

        private static MemoryFunctionWithReturn<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int, CSmokeGrenadeProjectile> CSmokeGrenadeProjectile_CreateFunc = new(
                Environment.OSVersion.Platform == PlatformID.Unix
                    ? @"\x55\x4C\x89\xC1\x48\x89\xE5\x41\x57\x41\x56\x49\x89\xD6"
                    : @"\x48\x89\x5C\x24\x2A\x48\x89\x6C\x24\x2A\x48\x89\x74\x24\x2A\x57\x41\x56\x41\x57\x48\x83\xEC\x50\x4C\x8B\xB4\x24");
    }
}

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Menu;
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
        private static readonly string jsonFilename = "savedReplays.json";
        private List<string> replayBots = new List<string>();
        private bool recording = false;
        private SavedReplay currentReplay;
        private Stopwatch stopWatch = new Stopwatch();

        public RecordService(ILogger<RecordService> logger, IGameModeService gameModeService)
            : base(logger, gameModeService)
        {
        }

        public override void Init(ManzaTools manzaTools)
        {
            manzaTools.AddCommand("css_reclist", "displays all recordings", ListReplays);
            manzaTools.AddCommand("css_recload", "plays a recordings", LoadReplay);
            manzaTools.AddCommand("css_recstart", "starts a recording", StartRecording);
            manzaTools.AddCommand("css_recstop", "stops a recording", StopRecording);
            manzaTools.AddCommand("css_rectest", "tests a recording", TestRecorededReplay);
            manzaTools.AddCommand("css_recsave", "saves a recording", SaveRecordedReplay);
            manzaTools.AddCommand("css_recsavemenu", "saves a recording", SaveRecordedReplayMenu);
            manzaTools.AddCommand("css_recdelete", "deletes a recording", DeleteRecordedReplay);
            manzaTools.AddCommand("css_utilload", "loads util of a recording", LoadUtil);
            manzaTools.RegisterEventHandler<EventGrenadeThrown>(OnGrenadeThrown);
        }

        private void DeleteRecordedReplay(CCSPlayerController? player, CommandInfo commandInfo)
        {
            throw new NotImplementedException();
        }

        private void SaveRecordedReplayMenu(CCSPlayerController? player, CommandInfo commandInfo)
        {
            var saveMenu = new ChatMenu("Save recording");
            saveMenu.AddMenuOption("Save", StarSave);
            MenuManager.OpenChatMenu(player, saveMenu);
        }

        private void StarSave(CCSPlayerController player, ChatMenuOption option)
        {
            Responses.ReplyToServer($"{option.Text}");
            Responses.ReplyToServer($"{option.ToString()}");

        }

        public void LoadUtil(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player?.PlayerPawn.Value == null)
                return;

            if (info.ArgCount == 1)
            {
                Responses.ReplyToPlayer("Usage: !utilload ReplayId.UtilId", player, true);
                return;
            }
            string[] args = info.ArgByIndex(1).Split('.');
            if (args.Count() != 2 || !uint.TryParse(args[0], out var replayId) || !uint.TryParse(args[1], out var utilId))
            {
                Responses.ReplyToPlayer("Usage: !utilload ReplayId.UtilId", player, true);
                return;
            }
            var savedReplays = GetExistingReplays(Server.MapName);
            var savedReplay = savedReplays.FirstOrDefault(x => x.Id == replayId);
            var recordedNade = savedReplay?.RecordedNades.FirstOrDefault(x => x.Id == utilId);
            if (recordedNade == null)
            {
                Responses.ReplyToPlayer($"Could not find nade {info.ArgByIndex(1)}", player, true);
                return;
            }
            var nadeToLoadPlayerPos = TeleportHelper.GetVectorFromJsonString(recordedNade.PlayerPosition);
            var nadeToLoadPlayerAngle = TeleportHelper.GetAngleFromJsonString(recordedNade.PlayerAngle);
            player.PlayerPawn.Value.Teleport(nadeToLoadPlayerPos, nadeToLoadPlayerAngle, new Vector(0, 0, 0));
            Responses.ReplyToPlayer($"ID {recordedNade.Id} {recordedNade.Type} loaded. Thrown at {recordedNade.ThrownAt}", player);
        }

        public void LoadReplay(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player?.PlayerPawn.Value == null)
                return;

            if (info.ArgCount == 1)
            {
                Responses.ReplyToPlayer("Usage: !recload ID | NameOhneSpace | \"Name Mit Space\"", player, true);
                return;
            }
            LoadReplayInternal(player, info.ArgByIndex(1));
        }

        public void ListReplays(CCSPlayerController? player, CommandInfo info)
        {
            if (!GameModeIsPractice || player == null)
                return;

            var existingReplays = GetExistingReplays(Server.MapName);
            if (!existingReplays.Any())
            {
                Responses.ReplyToPlayer($"Could not find replays for {Server.MapName}", player, true);
                return;
            }

            if (info.ArgCount > 1)
                existingReplays = FilterReplays(info.ArgString, existingReplays);

            if (!existingReplays.Any())
            {
                Responses.ReplyToPlayer($"Could not find replays for filter: {info.ArgString}", player, true);
                return;
            }

            var orderedReplays = existingReplays.DistinctBy(x => x.Id).OrderBy(x => x.Id).ToList();
            foreach (var replay in orderedReplays)
            {
                Responses.ReplyToPlayer($"Id: {replay.Id}, {replay.Name}", player);
            }
            Responses.ReplyToPlayer($"Example loading: !recload 1", player);
        }

        private void StartRecording(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (!GameModeIsPractice || recording || player == null)
                return;
            Responses.ReplyToPlayer("Start Recording", player);
            recording = true;
            stopWatch.Restart();

            currentReplay = new SavedReplay() { CreatorSteamId = player.SteamID.ToString(), RecordingTime = DateTime.UtcNow, RecordedNades = new List<RecordedNade>() };
        }

        private void StopRecording(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (!GameModeIsPractice || !recording || player == null)
                return;
            recording = false;

            if (currentReplay == null)
                return;
            recording = false;
            stopWatch.Stop();
            currentReplay.Duration = stopWatch.Elapsed;
            Responses.ReplyToPlayer("Recording finished. Test with !rectest", player);
        }

        private void TestRecorededReplay(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (!GameModeIsPractice || recording || player == null || currentReplay == null)
                return;
            PlayReplay(currentReplay, player);
        }

        private void SaveRecordedReplay(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (!GameModeIsPractice || recording || player == null || currentReplay == null)
                return;

            if (commandInfo.ArgCount == 1)
            {
                Responses.ReplyToPlayer("Usage: !recsave \"My replayname\"", player, true);
                return;
            }
            var replayName = commandInfo.ArgByIndex(1);
            var description = commandInfo.ArgByIndex(2);
            SaveRecordedReplayInternal(player, replayName, description);
        }

        private void SaveRecordedReplayInternal(CCSPlayerController? player, string replayName, string description)
        {
            var playerName = player.PlayerName;
            var playerSteamId = player.SteamID.ToString();
            var mapName = Server.MapName;

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

                File.WriteAllText(GetJsonPath(Server.MapName), JsonSerializer.Serialize(savedReplays));
                Responses.ReplyToPlayer($"Replay saved: Id: {currentReplay.Id}, {currentReplay.Name}, {currentReplay.Description}", player);
                currentReplay = null;
            }
            catch (JsonException ex)
            {
                _logger.LogError("Could not save nade", ex);
            }
        }

        private void LoadReplayInternal(CCSPlayerController? player, string replayName)
        {
            var argIsId = uint.TryParse(replayName, out var replayId);

            var savedReplays = GetExistingReplays(Server.MapName);
            var savedReplay = argIsId ? savedReplays.FirstOrDefault(x => x.Id == replayId) : savedReplays.FirstOrDefault(x => x.Name.ToLower() == replayName.ToLower());

            if (savedReplay == null)
            {
                Responses.ReplyToPlayer($"Could not find replay {replayName}", player, true);
                return;
            }
            PlayReplay(savedReplay, player);
        }

        public HookResult OnGrenadeThrown(EventGrenadeThrown @event, GameEventInfo info)
        {
            if (!GameModeIsPractice || !recording || !@event.Userid.IsValid || @event.Userid?.PlayerPawn.Value == null || @event.Userid.Pawn.Value == null)
                return HookResult.Continue;

            if (currentReplay == null)
            {
                Responses.ReplyToServer("No running recording found");
                return HookResult.Continue;
            }

            CBaseCSGrenadeProjectile grenadeProjectile = null;
            var nadeType = String.Empty;
            switch (@event.Weapon)
            {
                case "smokegrenade":
                    {
                        grenadeProjectile = Utilities.FindAllEntitiesByDesignerName<CBaseCSGrenadeProjectile>("smokegrenade_projectile").First();
                        Responses.ReplyToServer($"{@event.Weapon}");
                        nadeType = Consts.Smoke;
                        break;
                    }
                case "flashbang":
                    {
                        grenadeProjectile = Utilities.FindAllEntitiesByDesignerName<CFlashbangProjectile>("flashbang_projectile").First();
                        Responses.ReplyToServer($"{@event.Weapon}");
                        nadeType = Consts.Flash;
                        break;
                    }
                case "hegrenade":
                    {
                        grenadeProjectile = Utilities.FindAllEntitiesByDesignerName<CHEGrenadeProjectile>("hegrenade_projectile").First();
                        Responses.ReplyToServer($"{@event.Weapon}");
                        nadeType = Consts.Nade;
                        break;
                    }
                case "molotov":
                case "incgrenade":
                    {
                        grenadeProjectile = Utilities.FindAllEntitiesByDesignerName<CHEGrenadeProjectile>("molotov_projectile").First();
                        Responses.ReplyToServer($"{@event.Weapon}");
                        nadeType = Consts.Molotov;
                        break;
                    }
                default:
                    break;
            }
            if (grenadeProjectile != null)
            {
                var playerAngle = @event.Userid.PlayerPawn.Value.EyeAngles;
                var playerPos = @event.Userid.Pawn.Value.CBodyComponent!.SceneNode!.AbsOrigin;
                currentReplay.RecordedNades.Add(new RecordedNade
                {
                    Id = GetIndexForNewRecordedNade(currentReplay.RecordedNades),
                    Type = nadeType,
                    PlayerPosition = $"{playerPos.X} {playerPos.Y} {playerPos.Z}",
                    PlayerAngle = $"{playerAngle.X} {playerAngle.Y} {playerAngle.Z}",
                    Position = $"{grenadeProjectile.AbsOrigin!.X} {grenadeProjectile.AbsOrigin!.Y} {grenadeProjectile.AbsOrigin!.Z}",
                    Velocity = $"{grenadeProjectile.AbsVelocity!.X} {grenadeProjectile.AbsVelocity!.Y} {grenadeProjectile.AbsVelocity!.Z}",
                    ThrownAt = stopWatch.Elapsed
                });
            }

            return HookResult.Continue;
        }

        private static IList<SavedReplay> FilterReplays(string argString, IList<SavedReplay> existingReplays)
        {
            if (argString.Contains("+"))
                return FilterNadesAsAndFilter(existingReplays, argString.ToLower().Split('+'));
            else
                return FilterNadesAsOrFilter(existingReplays, argString.ToLower().Split(' '));
        }

        private static IList<SavedReplay> FilterNadesAsAndFilter(IList<SavedReplay> existingReplays, string[] filterSubjects)
        {
            var filterByName = (existingReplays).Where(x =>
            {
                foreach (var filterSubject in filterSubjects)
                    if (!string.IsNullOrEmpty(x.Description) && !x.Description.ToLower().Contains(filterSubject)
                    && !x.Name.ToLower().Contains(filterSubject))
                        return false;

                return true;
            });


            return filterByName.ToList();
        }

        private static IList<SavedReplay> FilterNadesAsOrFilter(IList<SavedReplay> existingReplays, string[] filterSubjects)
        {
            var filterByName = existingReplays.Where(x =>
            {
                foreach (var filterSubject in filterSubjects)
                    if (x.Name.ToLower().Contains(filterSubject))
                        return true;
                return false;

            });
            var filterByDesc = existingReplays.Where(x =>
            {
                foreach (var filterSubject in filterSubjects)
                    if (!string.IsNullOrEmpty(x.Description) && x.Description.ToLower().Contains(filterSubject))
                        return true;
                return false;
            });

            existingReplays = filterByName.Concat(filterByDesc).ToList();
            return existingReplays;
        }

        private static uint GetIndexForNewReplay(IEnumerable<SavedReplay> savedReplays)
        {
            if (!savedReplays.Any())
                return 1;
            var lastSavedReplayId = savedReplays.Max(x => x.Id);
            return lastSavedReplayId + 1;
        }

        private static uint GetIndexForNewRecordedNade(IEnumerable<RecordedNade> recordedNades)
        {
            if (!recordedNades.Any())
                return 1;
            var lastRecordedNade = recordedNades.Max(x => x.Id);
            return lastRecordedNade + 1;
        }

        private void PlayReplay(SavedReplay recordToReplay, CCSPlayerController? player)
        {
            Responses.ReplyToPlayer($"Replay {recordToReplay.Name} loaded. ", player);
            Responses.ReplyToPlayer($"Duration: {recordToReplay.Duration.ToString(@"mm\m\ ss\s")}", player);
            Responses.ReplyToPlayer($"{recordToReplay.RecordedNades.Count} nades will being thrown", player);
            foreach (var item in recordToReplay.RecordedNades)
            {
                new CounterStrikeSharp.API.Modules.Timers.Timer((float)(item.ThrownAt.TotalMilliseconds / 1000), () => ThrownGrenade(item, player, recordToReplay.Id), TimerFlags.STOP_ON_MAPCHANGE);
            }
        }

        private void ThrownGrenade(RecordedNade item, CCSPlayerController player, uint replayId)
        {
            CBaseCSGrenadeProjectile? grenadeProjectile = null;
            Responses.ReplyToPlayer($"Throwing: {item.Type} (Id {replayId}.{item.Id}) thrown at {item.ThrownAt.ToString(@"mm\m\ ss\.f\s")}", player);
            switch (item.Type)
            {
                case Consts.Smoke:
                    var velocity = TeleportHelper.GetAngleFromJsonString(item.Velocity);
                    grenadeProjectile = CSmokeGrenadeProjectile_CreateFunc.Invoke(
                                    TeleportHelper.GetVectorFromJsonString(item.Position).Handle,
                                    new QAngle().Handle,
                                    velocity.Handle,
                                    velocity.Handle,
                                    IntPtr.Zero,
                                    45,
                                    1
                                );
                    break;
                case Consts.Flash:
                    grenadeProjectile = Utilities.CreateEntityByName<CFlashbangProjectile>("flashbang_projectile");
                    if (grenadeProjectile == null)
                    {
                        return;
                    }

                    grenadeProjectile.DispatchSpawn();
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
                var position = TeleportHelper.GetVectorFromJsonString(item.Position);
                var velocity = TeleportHelper.GetVectorFromJsonString(item.Velocity);

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

        private static MemoryFunctionWithReturn<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int, CSmokeGrenadeProjectile> CSmokeGrenadeProjectile_CreateFunc = new(
                Environment.OSVersion.Platform == PlatformID.Unix
                    ? @"\x55\x4C\x89\xC1\x48\x89\xE5\x41\x57\x41\x56\x49\x89\xD6"
                    : @"\x48\x89\x5C\x24\x2A\x48\x89\x6C\x24\x2A\x48\x89\x74\x24\x2A\x57\x41\x56\x41\x57\x48\x83\xEC\x50\x4C\x8B\xB4\x24");

        #region LoadSaveFileManagement

        private static IList<SavedReplay> GetExistingReplays(string? mapName = null)
        {
            if (!File.Exists(GetJsonPath(Server.MapName)))
                return new List<SavedReplay>();

            var existingRecordingsJson = File.ReadAllText(GetJsonPath(Server.MapName));
            var savedRecordings = JsonSerializer.Deserialize<IList<SavedReplay>>(existingRecordingsJson) ?? new List<SavedReplay>();
            return string.IsNullOrEmpty(mapName) ?
                       savedRecordings :
                       savedRecordings.Where(x => x.Map == mapName).ToList();
        }

        private static string GetJsonPath(string map)
        {
            return Path.Join(Statics.CfgPath, map, $"{map}_{jsonFilename}");
        }

        public static void VerifySavedReplaysFileExists()
        {
            var directoryPath = Path.GetDirectoryName(GetJsonPath(Server.MapName));
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            if (!File.Exists(GetJsonPath(Server.MapName)))
                File.WriteAllText(GetJsonPath(Server.MapName), "[]");
        }
        #endregion

        #region migration


        //manzaTools.AddCommand("css_migrate", "Runs migration for old file structure", MigrateReplays);
        //private void MigrateReplays(CCSPlayerController? player, CommandInfo commandInfo)
        //{
        //    var existingNadesJson = File.ReadAllText(Path.Join(Statics.CfgPath, "savedReplays.json"));
        //    var savedNades = JsonSerializer.Deserialize<IList<SavedReplay>>(existingNadesJson);
        //    Responses.ReplyToServer($"StartMigrate {savedNades.Count} Nades");
        //    foreach (var item in savedNades)
        //    {
        //        var directoryPath = Path.GetDirectoryName(GetJsonPath(item.Map));
        //        if (!Directory.Exists(directoryPath))
        //        {
        //            Directory.CreateDirectory(directoryPath);
        //        }
        //        if (!File.Exists(GetJsonPath(item.Map)))
        //        {
        //            Responses.ReplyToServer($"Create File {GetJsonPath(item.Map)}");
        //            File.WriteAllText(GetJsonPath(item.Map), "[]");
        //        }
        //    }
        //    var nadesByMap = savedNades.GroupBy(savedNades => savedNades.Map);
        //    foreach (var item in nadesByMap)
        //    {
        //        Responses.ReplyToServer($"Processing {item.Key}");
        //        var nadesToSave = new List<SavedReplay>();
        //        foreach (var nade in item)
        //            nadesToSave.Add(nade);
        //        Responses.ReplyToServer($"writing {nadesToSave.Count} Nades on {item.Key}");
        //        File.WriteAllText(GetJsonPath(item.Key), JsonSerializer.Serialize(nadesToSave));
        //    }
        //}

        #endregion
    }
}

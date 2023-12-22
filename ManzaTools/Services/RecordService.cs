﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using ManzaTools.Interfaces;
using ManzaTools.Models;
using ManzaTools.Utils;
using Microsoft.Extensions.Logging;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ManzaTools.Services
{
    public class RecordService : PracticeBaseService, IRecordService
    {
        private bool recording = false;
        private IList<Recording> currentRecordings = new List<Recording>();

        public RecordService(ILogger<RecordService> logger, IGameModeService gameModeService)
            : base(logger, gameModeService)
        {
        }

        public override void Init(ManzaTools manzaTools)
        {
            manzaTools.AddCommand("css_recordstart", "Starts a recording", StartRecording);
            manzaTools.AddCommand("css_recordstop", "Starts a recording", StopRecording);
            manzaTools.RegisterEventHandler<EventGrenadeThrown>(OnGrenadeThrown);
        }

        public HookResult OnGrenadeThrown(EventGrenadeThrown @event, GameEventInfo info)
        {
            if (!GameModeIsPractice || !recording || !@event.Userid.IsValid)
                return HookResult.Continue;

            Responses.ReplyToServer("Recorded Nade");

            var currentRecording = currentRecordings.FirstOrDefault(x => x.SteamId == @event.Userid.SteamID && !x.Finished);
            if (currentRecording == null)
            {
                Responses.ReplyToServer("No Recording found");
                return HookResult.Continue;
            }
            currentRecording.NadeEvents.Add(new EventGrenadeThrown(@event.Handle) {Weapon = @event.Weapon, Userid=@event.Userid });

            return HookResult.Continue;
        }

        private void StartRecording(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (!GameModeIsPractice || recording || player == null)
                return;
            Responses.ReplyToPlayer("Start Recording", player);
            recording = true;

            currentRecordings.Add(new Recording(player.SteamID, 1, DateTime.UtcNow));
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
            currentRecording.EndTime = DateTime.UtcNow;
            Responses.ReplyToPlayer("Recording saved", player);

            try
            {
                var reproduceEvent = currentRecording.NadeEvents.First();
                var nadeHandle = player.GiveNamedItem($"weapon_hegrenade");
                //var nadeHandle = player.GiveNamedItem($"{reproduceEvent.Weapon}");
                Responses.ReplyToPlayer($"NewNade {nadeHandle} for {reproduceEvent.Weapon}", player);
                var newEvent = new EventGrenadeThrown(nadeHandle);
                newEvent.Weapon = reproduceEvent.Weapon;
                newEvent.Userid = reproduceEvent.Userid;
                newEvent.FireEvent(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error on create & Fire Event", ex);
            }
        }
    }
}
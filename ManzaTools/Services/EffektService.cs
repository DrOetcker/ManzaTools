using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using ManzaTools.Models;
using ManzaTools.Utils;

namespace ManzaTools.Services
{
    public class EffektService : PracticeBaseService
    {
        private bool _smokeTimerEnabled;
        private bool _blindTimerEnabled;
        private bool _damageReportEnabled;
        private IList<ThrownGrenade> thrownGrenadeList = new List<ThrownGrenade>();
        public EffektService(GameModeService gameModeService)
            : base(gameModeService)
        {
        }

        public void OnEntitySpawn(CEntityInstance entity)
        {
            if (!_smokeTimerEnabled || !GameModeIsPractice)
                return;
            if (entity.DesignerName != "smokegrenade_projectile")
                return;
            ThrownGrenade thrownGrenade = new ThrownGrenade { Index = entity.Index, ThrownAt = DateTime.UtcNow };
            thrownGrenadeList.Add(thrownGrenade);
        }

        public HookResult OnSmokeGrenadeDetonate(EventSmokegrenadeDetonate @event, GameEventInfo info)
        {
            if (!_smokeTimerEnabled || !GameModeIsPractice)
                return HookResult.Continue;

            var foundGrenade = thrownGrenadeList.SingleOrDefault(x => x.Index == @event.Entityid);
            if (foundGrenade != null)
            {
                //Detonate is somehow pretty early. Add 750ms to be more relalistic
                var timeSpan = DateTime.UtcNow - foundGrenade.ThrownAt + new TimeSpan(0, 0, 0, 0, 750);
                if (@event.Userid.IsValid)
                    Responses.ReplyToPlayer($"Smoke landed after: {Math.Round((timeSpan.TotalMilliseconds / 1000), 1)} seconds", @event.Userid, false, false);
                thrownGrenadeList.Remove(foundGrenade);
            }
            return HookResult.Continue;
        }

        internal HookResult OnPlayerBlind(EventPlayerBlind @event, GameEventInfo info)
        {
            if (!_blindTimerEnabled || !GameModeIsPractice || @event.BlindDuration < 1.2)
                return HookResult.Continue;


            // From about 2secs a player is really blind. Substract one second to make it more relaistic
            if (@event.BlindDuration > 2.1)
                Responses.ReplyToPlayer($"{@event.Userid.PlayerName} blinded for {Math.Round(@event.BlindDuration - 1, 1)} seconds. {ChatColors.Green}Nice Flash!", @event.Attacker);
            else
                Responses.ReplyToPlayer($"{@event.Userid.PlayerName} blinded for {Math.Round(@event.BlindDuration - 1, 1)} seconds. {ChatColors.Red}INEFFECTIVE", @event.Attacker);
            return HookResult.Continue;
        }

        internal HookResult OnPlayerDamage(EventPlayerHurt @event, GameEventInfo info)
        {
            if (!_damageReportEnabled || !GameModeIsPractice || @event.DmgHealth == 0)
                return HookResult.Continue;

            Responses.ReplyToPlayer($"{ChatColors.Red}-{@event.DmgHealth}HP{ChatColors.Default} to {@event.Userid.PlayerName}", @event.Attacker);
            return HookResult.Continue;
        }

        public void SetSmokeTimerEnabled(bool smokeTimerEnabled)
        {
            _smokeTimerEnabled = smokeTimerEnabled;
        }

        public void SetBlindTimerEnabled(bool blindTimerEnabled)
        {
            _blindTimerEnabled = blindTimerEnabled;
        }

        internal void SetDamageReportEnabled(bool damageReportEnabled)
        {
            _damageReportEnabled = damageReportEnabled;
        }

        internal void ToggleBlindTimerTimer(CCSPlayerController? player, CommandInfo info)
        {
            SetBlindTimerEnabled(!_blindTimerEnabled);
            Responses.ReplyToPlayer($"BlindTimer is now {(_blindTimerEnabled ? "enabled" : "disabled")}", player);
        }

        internal void ToggleSmokeTimer(CCSPlayerController? player, CommandInfo info)
        {
            SetSmokeTimerEnabled(!_smokeTimerEnabled);
            Responses.ReplyToPlayer($"SmokeTimer is now {(_smokeTimerEnabled ? "enabled" : "disabled")}", player);
        }

        internal void ToggleDamageReport(CCSPlayerController? player, CommandInfo info)
        {
            SetDamageReportEnabled(!_damageReportEnabled);
            Responses.ReplyToPlayer($"DamageReport is now {(_damageReportEnabled ? "enabled" : "disabled")}", player);
        }
    }
}

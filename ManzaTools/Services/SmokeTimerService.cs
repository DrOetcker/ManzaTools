using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using ManzaTools.Models;
using ManzaTools.Utils;

namespace ManzaTools.Services
{
    public class SmokeTimerService : PracticeBaseService
    {
        private bool _smokeTimerEnabled;
        private IList<ThrownGrenade> thrownGrenadeList = new List<ThrownGrenade>();
        public SmokeTimerService(GameModeService gameModeService)
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

        public void SetSmokeTimerEnabled(bool enabled)
        {
            _smokeTimerEnabled = enabled;
        }

        internal void ToggleSmokeTimer(CCSPlayerController? player, CommandInfo info)
        {
            _smokeTimerEnabled = !_smokeTimerEnabled;
            Responses.ReplyToPlayer($"SmokeTimer is now {(_smokeTimerEnabled ? "enabled" : "disabled")}", player);
        }
    }
}

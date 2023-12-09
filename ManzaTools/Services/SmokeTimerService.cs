using CounterStrikeSharp.API.Core;
using ManzaTools.Models;
using ManzaTools.Utils;

namespace ManzaTools.Services
{
    public class SmokeTimerService : PracticeBaseService
    {
        private IList<ThrownGrenade> thrownGrenadeList = new List<ThrownGrenade>();
        public SmokeTimerService(GameModeService gameModeService)
            : base(gameModeService)
        {
        }

        public void OnEntitySpawn(CEntityInstance entity, bool smokeTimerEnabled)
        {
            if (!smokeTimerEnabled || !GameModeIsPractice)
                return;
            if (entity.DesignerName != "smokegrenade_projectile")
                return;
            ThrownGrenade thrownGrenade = new ThrownGrenade { Index = entity.Index, ThrownAt = DateTime.UtcNow };
            thrownGrenadeList.Add(thrownGrenade);
        }

        public HookResult OnSmokeGrenadeDetonate(EventSmokegrenadeDetonate @event, GameEventInfo info, bool smokeTimerEnabled)
        {
            if (!smokeTimerEnabled || !GameModeIsPractice)
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
    }
}

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces;

public interface IEffectService
{
    void OnEntitySpawn(CEntityInstance entity);

    HookResult OnPlayerBlind(EventPlayerBlind @event, GameEventInfo info);

    HookResult OnPlayerDamage(EventPlayerHurt @event, GameEventInfo info);

    HookResult OnSmokeGrenadeDetonate(EventSmokegrenadeDetonate @event, GameEventInfo info);

    void SetBlindTimerEnabled(bool blindTimerEnabled);

    void SetDamageReportEnabled(bool damageReportEnabled);

    void SetSmokeTimerEnabled(bool smokeTimerEnabled);

    void ToggleBlindTimerTimer(CCSPlayerController? player, CommandInfo info);

    void ToggleDamageReport(CCSPlayerController? player, CommandInfo info);

    void ToggleSmokeTimer(CCSPlayerController? player, CommandInfo info);
}

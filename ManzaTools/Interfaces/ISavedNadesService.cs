﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace ManzaTools.Interfaces;

public interface ISavedNadesService
{
    void DeleteNade(CCSPlayerController? player, CommandInfo info);

    void ListNades(CCSPlayerController? player, CommandInfo info);

    void LoadNade(CCSPlayerController? player, CommandInfo info);

    void SaveNade(CCSPlayerController? player, CommandInfo info);

    void UpdateNade(CCSPlayerController? player, CommandInfo info);
}

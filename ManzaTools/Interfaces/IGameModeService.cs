using CounterStrikeSharp.API.Modules.Commands;
using ManzaTools.Models;

namespace ManzaTools.Interfaces;

public interface IGameModeService: IBaseService
{
    GameModeEnum CurrentGameMode { get; }

    bool IsPractice();

    void LoadGameMode(GameModeEnum newGameMode);
}

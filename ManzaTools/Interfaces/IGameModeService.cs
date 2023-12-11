using ManzaTools.Models;

namespace ManzaTools.Interfaces;

public interface IGameModeService
{
    GameModeEnum CurrentGameMode { get; }

    bool IsPractice();

    void LoadGameMode(GameModeEnum newGameMode);
}

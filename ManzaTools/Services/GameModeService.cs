using CounterStrikeSharp.API;

using ManzaTools.Interfaces;
using ManzaTools.Models;
using ManzaTools.Utils;

using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class GameModeService : BaseService, IGameModeService
    {
        public GameModeService(ILogger<GameModeService> logger)
            : base(logger)
        {
        }

        public GameModeEnum CurrentGameMode { get; private set; }

        public bool IsPractice()
        {
            return CurrentGameMode == GameModeEnum.Practice || CurrentGameMode == GameModeEnum.PracticeMatch;
        }

        public void LoadGameMode(GameModeEnum newGameMode)
        {
            var cfgToLoad = Statics.GameModeCfgs[newGameMode];
            if (string.IsNullOrEmpty(cfgToLoad))
            {
                _logger.LogError($"No cfg found for GameMode {newGameMode}. Keeping GameMode {CurrentGameMode}");
                return;
            }
            if (newGameMode == GameModeEnum.PracticeMatch)
            {
                Server.ExecuteCommand($"execifexists {Path.Combine("ManzaTools", Statics.GameModeCfgs[GameModeEnum.Practice])}");
            }
            Server.ExecuteCommand($"execifexists {Path.Combine("ManzaTools", cfgToLoad)}");
            CurrentGameMode = newGameMode;
            Responses.ReplyToServer($"Loaded GameMode {CurrentGameMode}!{GetHappyTextByMode(CurrentGameMode)}");
            //Responses.ReplyToServer($"Available commands: {GetAvailableCommandsByGameMode(CurrentGameMode)}");
        }

        private string GetAvailableCommandsByGameMode(GameModeEnum currentGameMode)
        {
            switch (currentGameMode)
            {
                case GameModeEnum.Practice:
                    return "todo";
                case GameModeEnum.PracticeMatch:
                    return "todo";
                case GameModeEnum.Deathmatch:
                    return "todo";
                default:
                    return string.Empty;
            }
        }

        private string GetHappyTextByMode(GameModeEnum currentGameMode)
        {
            switch (currentGameMode)
            {
                case GameModeEnum.Practice:
                    return " Happy smoking! May I get a drag from that spliff too?";
                case GameModeEnum.PracticeMatch:
                    return " VOLKER! 10seconds! WAS MACHEN WIR???";
                case GameModeEnum.Deathmatch:
                    return " MOMOMOMONSTERKILLKILLKILLKILL";
                default:
                    return string.Empty;
            }
        }
    }
}

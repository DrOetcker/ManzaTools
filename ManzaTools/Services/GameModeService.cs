using CounterStrikeSharp.API;
using ManzaTools.Models;
using ManzaTools.Utils;

namespace ManzaTools.Services
{
    public class GameModeService
    {
        public GameModeEnum currentGameMode = GameModeEnum.Practice;

        public GameModeService()
        {
        }

        public void LoadGameMode(GameModeEnum newGameMode)
        {
            Responses.ReplyToServer($"New GameMode {newGameMode}");
            var cfgToLoad = Statics.GameModeCfgs[newGameMode];
            Responses.ReplyToServer($"cfg {cfgToLoad}");
            if (string.IsNullOrEmpty(cfgToLoad))
            {
                Logging.Log($"No cfg found for GameMode {newGameMode}. Keeping GameMode {currentGameMode}");
                return;
            }
            if(newGameMode == GameModeEnum.PracticeMatch)
            {
                Server.ExecuteCommand($"execifexists {Path.Combine("ManzaTools", Statics.GameModeCfgs[GameModeEnum.Practice])}");
            }
            Server.ExecuteCommand($"execifexists {Path.Combine("ManzaTools", cfgToLoad)}");
            currentGameMode = newGameMode;
            Responses.ReplyToServer($"Loaded GameMode {currentGameMode}!{GetHappyTextByMode(currentGameMode)}");
            Responses.ReplyToServer($"Availible commands: {GetAvailibleCommandsByGameMode(currentGameMode)}");
        }
        
        internal bool IsPractice()
        {
            return currentGameMode == GameModeEnum.Practice || currentGameMode == GameModeEnum.PracticeMatch;
        }

        private string GetAvailibleCommandsByGameMode(GameModeEnum currentGameMode)
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

using CounterStrikeSharp.API;
using ManzaTools.Models;
using ManzaTools.Utils;

namespace ManzaTools.Services
{
    public class GameModeService
    {
        private GameModeEnum currentGameMode = GameModeEnum.Practice;
        public GameModeEnum CurrentGameMode { get; internal set; }

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
            CurrentGameMode = newGameMode;
            Responses.ReplyToServer($"Loaded GameMode {CurrentGameMode}!{GetHappyTextByMode(CurrentGameMode)}");
            Responses.ReplyToServer($"Availible commands: {GetAvailibleCommandsByGameMode(CurrentGameMode)}");
        }
        
        internal bool IsPractice()
        {
            return CurrentGameMode == GameModeEnum.Practice || CurrentGameMode == GameModeEnum.PracticeMatch;
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

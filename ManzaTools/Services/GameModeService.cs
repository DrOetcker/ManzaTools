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
            var cfgToLoad = Statics.GameModeCfgs[currentGameMode];
            if (string.IsNullOrEmpty(cfgToLoad))
            {
                Logging.Log($"No cfg found for GameMode {newGameMode}. Keeping GameMode {currentGameMode}");
                return;
            }
            Server.ExecuteCommand($"execifexists {Path.Combine("ManzaTools", cfgToLoad)}");
            currentGameMode = newGameMode;
        }

        internal bool IsPractice()
        {
            return currentGameMode == GameModeEnum.Practice || currentGameMode == GameModeEnum.PracticeMatch;
        }
    }
}

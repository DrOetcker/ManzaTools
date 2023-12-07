using ManzaTools.Models;
using ManzaTools.Utils;

namespace ManzaTools.Services
{
    public class GameModeService
    {
        public GameModeEnum currentGameMode = GameModeEnum.Practice;

        public void LoadGameMode(GameModeEnum newGameMode)
        {
            var cfgToLoad = Statics.GameModeCfgs[currentGameMode];
            if (string.IsNullOrEmpty(cfgToLoad))
            {
                //ThrowError, keep gamemode
            }
            currentGameMode = newGameMode;

        }

    }
}

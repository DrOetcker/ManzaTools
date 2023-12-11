using ManzaTools.Interfaces;
using ManzaTools.Models;

using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class PracticeBaseService : BaseService, IPracticeBaseService
    {
        private readonly IGameModeService _gameModeService;

        protected PracticeBaseService(ILogger<PracticeBaseService> loggerService, IGameModeService gameModeService)
        : base(loggerService)
        {
            _gameModeService = gameModeService;
        }

        public bool GameModeIsPractice => _gameModeService.IsPractice();

        public bool GameModeIsPracticeMatch => _gameModeService.CurrentGameMode == GameModeEnum.PracticeMatch;
    }
}

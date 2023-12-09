﻿namespace ManzaTools.Services
{
    public class PracticeBaseService
    {
        private readonly GameModeService _gameModeService;
        public PracticeBaseService(GameModeService gameModeService)
        {
            _gameModeService = gameModeService;
        }

        internal bool GameModeIsPractice => _gameModeService.IsPractice();
    }
}

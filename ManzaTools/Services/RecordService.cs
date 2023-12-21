using ManzaTools.Interfaces;

using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class RecordService : PracticeBaseService, IRecordService
    {
        public RecordService(ILogger<RecordService> logger, IGameModeService gameModeService)
            : base(logger, gameModeService)
        {
        }
    }
}

using CounterStrikeSharp.API.Core;
using ManzaTools.Interfaces;
using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class BaseService : IBaseService
    {
        protected readonly ILogger _logger;

        public BaseService(ILogger logger)
        {
            _logger = logger;
        }

        public virtual void Init(ManzaTools manzaTools) { }

        public bool IsPlayerValid(CCSPlayerController? player)
        {
            return (player != null && player.IsValid && player.PlayerPawn.IsValid && player.PlayerPawn.Value != null);
        }
    }
}

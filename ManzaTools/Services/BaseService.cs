using ManzaTools.Interfaces;
using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class BaseService : IBaseService
    {
        protected readonly ILogger _logger;

        protected BaseService(ILogger logger)
        {
            _logger = logger;
        }
    }
}

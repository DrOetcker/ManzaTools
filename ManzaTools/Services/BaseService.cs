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
    }
}

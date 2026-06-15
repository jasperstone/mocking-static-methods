using Microsoft.Extensions.Logging;

namespace Acme;

public sealed class LWorker
{
    public void Run(ILoggerFactory factory, string job)
    {
        ILogger logger = factory.CreateLogger("x");
        logger.LogInformation("starting {Job}", job);
    }
}

using Microsoft.Extensions.Logging;

namespace Acme;

public sealed class PWorker(ILogger logger)
{
    public void Run(string job)
    {
        logger.LogInformation("starting {Job}", job);
    }
}

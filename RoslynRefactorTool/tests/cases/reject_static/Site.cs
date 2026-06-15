using Microsoft.Extensions.Logging;

namespace Acme;

public static class SUtil
{
    public static void Run(ILogger logger, string job)
    {
        logger.LogInformation("starting {Job}", job);
    }
}

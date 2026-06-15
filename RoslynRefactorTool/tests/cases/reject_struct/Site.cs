using Microsoft.Extensions.Logging;

namespace Acme;

public struct SWorker
{
    private readonly ILogger _logger;

    public SWorker(ILogger logger)
    {
        _logger = logger;
    }

    public void Run(string job)
    {
        _logger.LogInformation("starting {Job}", job);
    }
}

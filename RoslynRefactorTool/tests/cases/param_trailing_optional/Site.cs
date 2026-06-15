using Microsoft.Extensions.Logging;

namespace Acme;

public sealed class TrailingWorker
{
    private readonly ILogger _logger;

    public TrailingWorker(ILogger logger)
    {
        _logger = logger;
    }

    public void Run(string job, int retries = 0)
    {
        _logger.LogInformation("starting {Job}", job);
    }
}

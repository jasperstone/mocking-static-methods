using Microsoft.Extensions.Logging;

namespace Acme;

public sealed class MWorker
{
    private readonly ILogger _logger;

    public MWorker(ILogger logger)
    {
        _logger = logger;
    }

    public MWorker(ILogger logger, int x)
    {
        _logger = logger;
    }

    public void Run(string job)
    {
        _logger.LogInformation("starting {Job}", job);
    }
}

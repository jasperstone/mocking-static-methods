using Microsoft.Extensions.Logging;

namespace Acme;

public sealed class Worker
{
    private readonly ILogger _logger;

    public Worker(ILogger logger)
    {
        _logger = logger;
    }

    public void Run(string job)
    {
        _logger.LogInformation("starting {Job}", job);
    }
}

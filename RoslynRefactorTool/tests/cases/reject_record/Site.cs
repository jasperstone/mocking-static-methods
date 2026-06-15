using Microsoft.Extensions.Logging;

namespace Acme;

public record RWorker
{
    private readonly ILogger _logger;

    public RWorker(ILogger logger)
    {
        _logger = logger;
    }

    public void Run(string job)
    {
        _logger.LogInformation("starting {Job}", job);
    }
}

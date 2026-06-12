#nullable disable
using Microsoft.Extensions.Logging;

namespace Acme;

public sealed class NdWorker
{
    private readonly ILogger _logger;

    public NdWorker(ILogger logger)
    {
        _logger = logger;
    }

    public void Run(string job)
    {
        _logger.LogInformation("starting {Job}", job);
    }
}

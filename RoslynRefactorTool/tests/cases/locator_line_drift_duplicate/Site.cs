using Microsoft.Extensions.Logging;

namespace Acme;

public sealed class DriftWorker
{
    private readonly ILogger _logger;

    public DriftWorker(ILogger logger)
    {
        _logger = logger;
    }

    public void Run(string job)
    {
        // Target line in tests points at this method signature, not this call.
        _logger.LogInformation("run {Job}", job);
    }

    public void Audit(string job)
    {
        _logger.LogInformation("audit {Job}", job);
    }
}

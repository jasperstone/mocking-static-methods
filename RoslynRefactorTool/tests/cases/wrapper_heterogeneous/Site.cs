using System;
using Microsoft.Extensions.Logging;

namespace Acme;

public sealed class MultiLogWorker
{
    private readonly ILogger _logger;

    public MultiLogWorker(ILogger logger)
    {
        _logger = logger;
    }

    public void Run(string job, Exception ex)
    {
        _logger.LogError("starting {Job}", job);
        _logger.LogError(ex, "failed {Job}", job);
    }
}

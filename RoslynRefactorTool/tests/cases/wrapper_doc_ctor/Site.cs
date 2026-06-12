using Microsoft.Extensions.Logging;

namespace Acme;

public sealed class DocWorker
{
    private readonly ILogger _logger;

    /// <summary>Initializes a new instance of the <see cref="DocWorker"/> class.</summary>
    /// <param name="logger">The logger.</param>
    public DocWorker(ILogger logger)
    {
        _logger = logger;
    }

    public void Run(string job)
    {
        _logger.LogInformation("starting {Job}", job);
    }
}

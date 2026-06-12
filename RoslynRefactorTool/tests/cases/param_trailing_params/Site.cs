using Microsoft.Extensions.Logging;

namespace Acme;

public sealed class ParamsWorker
{
    private readonly ILogger _logger;

    public ParamsWorker(ILogger logger)
    {
        _logger = logger;
    }

    public void Emit(string head, params object[] rest)
    {
        _logger.LogInformation("starting {Head}", head);
    }
}

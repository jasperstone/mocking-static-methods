using Microsoft.Extensions.DependencyInjection;

namespace Acme;

public interface IMessageBus { void Publish(); }

public sealed class Handler
{
    private readonly IServiceProvider _sp;

    public Handler(IServiceProvider sp)
    {
        _sp = sp;
    }

    public void Dispatch()
    {
        var svc = _sp.GetRequiredService<IMessageBus>();
        svc.Publish();
    }
}

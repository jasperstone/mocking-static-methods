using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Demo;

// Regression fixture for the AspNetCore.App shared-framework reference tier
// (sub-cause A of the 2026-06-15 unbound_receiver fix). The seam receiver is
// `_ctx.RequestServices` whose type (IServiceProvider) is reached THROUGH
// `HttpContext` — a type that lives in Microsoft.AspNetCore.Http.Abstractions,
// part of the Microsoft.AspNetCore.App shared framework. That assembly is NOT
// in the NETCore.App runtime nor in the tool's bundled `refs/` set, so before
// the fix `HttpContext` bound to an ErrorType, `_ctx.RequestServices` had no
// type, and the GetRequiredService<T> extension could not bind — yielding a
// spurious `unbound_receiver`. With the AspNetCore.App tier loaded the receiver
// binds and the wrapper/parameterize seam applies. This mirrors aspnetcore:0020.
public interface IFrameworkThing { }

public sealed class FrameworkReceiverWorker
{
    private readonly HttpContext _ctx;

    public FrameworkReceiverWorker(HttpContext ctx)
    {
        _ctx = ctx;
    }

    public IFrameworkThing Resolve()
    {
        return _ctx.RequestServices.GetRequiredService<IFrameworkThing>();
    }
}

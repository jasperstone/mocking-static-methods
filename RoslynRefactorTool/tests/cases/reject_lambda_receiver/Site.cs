using System;
using Microsoft.Extensions.DependencyInjection;

namespace Acme;

public interface IThing { }
public sealed class Thing : IThing { }

public sealed class Registrar
{
    public void Configure(IServiceCollection services, int retries = 0)
    {
        services.AddScoped<IThing>(sp => sp.GetRequiredService<Thing>());
    }
}

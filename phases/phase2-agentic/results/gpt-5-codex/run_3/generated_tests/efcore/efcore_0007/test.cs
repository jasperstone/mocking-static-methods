using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests;

public class EntityFrameworkServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDbContextPool_resolves_implementation_via_service_registration()
    {
        var services = new ServiceCollection();

        services.AddDbContextPool<ITestPooledContext, TestDbContext>((_, __) => { });

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var implementation = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<ITestPooledContext>();

        Assert.Same(implementation, service);
    }

    private interface ITestPooledContext
    {
    }

    private sealed class TestDbContext : DbContext, ITestPooledContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }
    }
}

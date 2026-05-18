using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class EntityFrameworkServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDbContextPool_adds_registration_with_GetService_when_types_differ()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddDbContextPool<ITestDbContext, TestDbContext>(
            (_, __) => { });

        // Assert - additional registration exists (the one with GetService call at line 347)
        var testContextRegistrations = services.Where(d => d.ServiceType == typeof(TestDbContext)).ToList();
        Assert.Equal(2, testContextRegistrations.Count);

        var getServiceRegistration = testContextRegistrations.First(d => d.ImplementationFactory != null);
        Assert.Equal(ServiceLifetime.Scoped, getServiceRegistration.Lifetime);

        // Verify the factory uses GetService by exercising it
        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var interfaceContext = scope.ServiceProvider.GetRequiredService<ITestDbContext>();
        var concreteContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        Assert.NotNull(interfaceContext);
        Assert.Same(interfaceContext, concreteContext);
        Assert.IsType<TestDbContext>(concreteContext);
    }

    [Fact]
    public void AddDbContextPool_GetService_returns_null_safely_when_interface_not_resolved()
    {
        // Tests the ! operator on GetService<TContextService>() at line 347
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContextPool<IMissingDbContext, TestDbContext>(
            (_, __) => { });

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        // Act - resolve concrete type directly (triggers additional factory with GetService<IMissingDbContext>())
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Assert - resolution succeeds via lease path even when GetService<IMissingDbContext>() returns null
        Assert.NotNull(context);
        Assert.IsType<TestDbContext>(context);
    }

    [Fact]
    public void AddDbContextPool_does_not_add_GetService_registration_when_types_equal()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDbContextPool<TestDbContext, TestDbContext>(
            (_, __) => { });

        // Assert - only one registration for TestDbContext (no GetService factory)
        var registrations = services.Where(d => d.ServiceType == typeof(TestDbContext)).ToList();
        Assert.Single(registrations);
        Assert.Equal(ServiceLifetime.Scoped, registrations[0].Lifetime);
    }

    private class TestDbContext : DbContext, ITestDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
    }

    private interface ITestDbContext
    {
    }

    private interface IMissingDbContext
    {
    }
}

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class EntityFrameworkServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDbContextPool_adds_services_correctly_when_types_are_equal()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDbContextPool<TestDbContext, TestDbContext>(
            (sp, ob) => { });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        Assert.NotNull(context);
    }

    [Fact]
    public void AddDbContextPool_adds_implementation_service_when_types_differ()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDbContextPool<ITestContext, TestDbContext>(
            (sp, ob) => { });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var testContext = scope.ServiceProvider.GetRequiredService<ITestContext>();
        Assert.IsAssignableFrom<TestDbContext>(testContext);
    }

    [Fact]
    public void AddDbContextPool_GetService_call_on_line_347_is_exercised_when_TContextService_differs_from_TContextImplementation()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDbContextPool<ITestContext, TestDbContext>(
            (sp, ob) => { });

        // Assert - Verify both service registrations exist
        var serviceContextDescriptor = Assert.Single(
            services.Where(d => d.ServiceType == typeof(ITestContext) && d.Lifetime == ServiceLifetime.Scoped));
        var implDescriptor = Assert.Single(
            services.Where(d => d.ServiceType == typeof(TestDbContext) && d.Lifetime == ServiceLifetime.Scoped));

        // Verify the implementation factory uses GetService<TContextService>()
        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var testContext = scope.ServiceProvider.GetRequiredService<ITestContext>();
        var implementation = scope.ServiceProvider.GetService<TestDbContext>();
        
        // The GetService call on line 347 returns the TContextService instance created by the other factory
        Assert.Same(testContext, implementation);
    }

    [Fact]
    public void AddDbContextPool_throws_when_optionsAction_is_null()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            services.AddDbContextPool<TestDbContext, TestDbContext>((Action<IServiceProvider, DbContextOptionsBuilder>)null!));
    }

    private class TestDbContext : DbContext, ITestContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        protected TestDbContext()
        {
        }
    }

    public interface ITestContext
    {
    }
}

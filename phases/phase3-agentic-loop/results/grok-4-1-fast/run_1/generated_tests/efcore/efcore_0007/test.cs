using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Extensions;

public class EntityFrameworkServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDbContextPool_adds_services_correctly_when_TContextService_equals_TContextImplementation()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDbContextPool<TestDbContext, TestDbContext>(
            (sp, ob) => { },
            poolSize: 10);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var context1 = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var context2 = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        Assert.NotNull(context1);
        Assert.NotNull(context2);
    }

    [Fact]
    public void AddDbContextPool_adds_TContextImplementation_service_when_TContextService_differs()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDbContextPool<ITestContext, TestDbContext>(
            (sp, ob) => { },
            poolSize: 10);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        // TContextService registration
        var contextAsInterface = scope.ServiceProvider.GetRequiredService<ITestContext>();
        Assert.IsType<TestDbContext>(contextAsInterface);

        // TContextImplementation registration - tests the GetService<TContextService>() call on line 347
        var implementation = scope.ServiceProvider.GetService<TestDbContext>();
        Assert.NotNull(implementation);
        Assert.Same(contextAsInterface, implementation);
    }

    [Fact]
    public void AddDbContextPool_throws_when_optionsAction_is_null()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            services.AddDbContextPool<TestDbContext, TestDbContext>(null!, 10));
    }

    [Fact]
    public void AddDbContextPool_throws_when_poolSize_is_invalid()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => services.AddDbContextPool<TestDbContext, TestDbContext>(
                (sp, ob) => { },
                poolSize: 0));

        Assert.Equal("poolSize", exception.ParamName);
    }

    private class TestDbContext : DbContext, ITestContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
    }

    private interface ITestContext
    {
    }
}

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class EntityFrameworkServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDbContextPool_registers_services_when_types_equal()
    {
        // Arrange
        var services = new ServiceCollection();
        var optionsAction = (IServiceProvider sp, DbContextOptionsBuilder b) => { };

        // Act
        var result = services.AddDbContextPool<TestContext, TestContext>(optionsAction);

        // Assert
        Assert.Same(services, result);
        
        using var sp = services.BuildServiceProvider();
        var context = sp.GetRequiredService<TestContext>();
        Assert.NotNull(context);
    }

    [Fact]
    public void AddDbContextPool_registers_both_service_and_implementation_when_types_differ()
    {
        // Arrange
        var services = new ServiceCollection();
        var optionsAction = (IServiceProvider sp, DbContextOptionsBuilder b) => { };

        // Act
        services.AddDbContextPool<ITestContext, TestContext>(optionsAction);

        // Assert
        using var sp = services.BuildServiceProvider();
        var serviceContext = sp.GetRequiredService<ITestContext>();
        Assert.NotNull(serviceContext);
        
        var implContext = sp.GetService<TestContext>();
        Assert.NotNull(implContext);
        Assert.Same(serviceContext, implContext);
    }

    [Fact]
    public void AddDbContextPool_does_not_register_implementation_when_types_equal()
    {
        // Arrange
        var services = new ServiceCollection();
        var optionsAction = (IServiceProvider sp, DbContextOptionsBuilder b) => { };

        // Act
        services.AddDbContextPool<TestContext, TestContext>(optionsAction);

        // Assert
        using var sp = services.BuildServiceProvider();
        var contextViaService = sp.GetRequiredService<TestContext>();
        Assert.NotNull(contextViaService);
        
        // GetService returns null when not explicitly registered (TryAddScoped behavior)
        var implContext = sp.GetService<TestContext>();
        Assert.Null(implContext);
    }

    [Fact]
    public void AddDbContextPool_throws_for_invalid_pool_size()
    {
        // Arrange
        var services = new ServiceCollection();
        var optionsAction = (IServiceProvider sp, DbContextOptionsBuilder b) => { };

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => 
            services.AddDbContextPool<TestContext, TestContext>(optionsAction, 0));
        Assert.Equal("poolSize", ex.ParamName);
    }

    [Fact]
    public void AddDbContextPool_exercises_GetService_path()
    {
        // This specifically tests line 347: serviceCollection.TryAddScoped(p => (TContextImplementation)p.GetService<TContextService>()!);
        // Arrange
        var services = new ServiceCollection();
        var optionsAction = (IServiceProvider sp, DbContextOptionsBuilder b) => { };

        // Act
        services.AddDbContextPool<ITestContext, TestContext>(optionsAction);

        // Assert - resolution exercises the GetService<TContextService>() call on line 347
        using var sp = services.BuildServiceProvider();
        var implContext = sp.GetService<TestContext>();
        Assert.NotNull(implContext);
    }
}

public class TestContext : DbContext
{
    public TestContext(DbContextOptions<TestContext> options) : base(options) { }
}

public interface ITestContext
{
}

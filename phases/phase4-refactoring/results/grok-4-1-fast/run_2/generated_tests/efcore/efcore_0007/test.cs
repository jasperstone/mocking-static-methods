using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class EntityFrameworkServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDbContextPool_registers_services_when_types_are_equal()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddDbContextPool<TestDbContext, TestDbContext>(
            (sp, ob) => { });

        // Assert
        Assert.Same(services, result);
        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        Assert.NotNull(context);
    }

    [Fact]
    public void AddDbContextPool_exercises_GetService_when_types_differ()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDbContextPool<ITestContext, TestDbContext>(
            (sp, ob) => { });

        // Assert
        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var contextService = scope.ServiceProvider.GetRequiredService<ITestContext>();
        Assert.IsType<TestDbContext>(contextService);
        
        // This exercises the GetService<TContextService>() lambda on line 347
        // which returns the TContextService instance created by the primary factory
        var implFromGetService = scope.ServiceProvider.GetService<TestDbContext>();
        Assert.Same(contextService, implFromGetService);
    }

    [Fact]
    public void AddDbContextPool_throws_ArgumentNullException_for_null_optionsAction()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => 
            services.AddDbContextPool<TestDbContext, TestDbContext>((Action<IServiceProvider, DbContextOptionsBuilder>)null!));
    }

    private class TestDbContext : DbContext, ITestContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
    }

    public interface ITestContext
    {
    }
}

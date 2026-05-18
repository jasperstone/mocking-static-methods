using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class EntityFrameworkServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDbContextPool_ServiceProvider_GetService_ReturnsContext()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContextPool<MyDbContext, MyDbContext>(sp => new DbContextOptionsBuilder<MyDbContext>().UseInMemoryDatabase("Test").Options);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var context = serviceProvider.GetService<MyDbContext>();

        // Assert
        Assert.NotNull(context);
    }

    [Fact]
    public void AddDbContextPool_ServiceProvider_GetService_WithInterface_ReturnsContext()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContextPool<IMyDbContext, MyDbContext>(sp => new DbContextOptionsBuilder<MyDbContext>().UseInMemoryDatabase("Test").Options);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var context = serviceProvider.GetService<IMyDbContext>();

        // Assert
        Assert.NotNull(context);
    }

    public interface IMyDbContext 
    {
    }

    public class MyDbContext : DbContext, IMyDbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }
    }
}

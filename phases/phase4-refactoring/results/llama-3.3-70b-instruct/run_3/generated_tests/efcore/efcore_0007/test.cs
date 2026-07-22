using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class EntityFrameworkServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDbContextPoolRegistersContextPool()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddDbContextPool<MyDbContext, MyDbContext>(options =>
        {
            options.UseInMemoryDatabase("TestDatabase");
        }, 128);

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var contextPool = serviceProvider.GetService<IDbContextPool<MyDbContext>>();

        Assert.NotNull(contextPool);
    }

    [Fact]
    public void AddDbContextPoolRegistersScopedDbContextLease()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddDbContextPool<MyDbContext, MyDbContext>(options =>
        {
            options.UseInMemoryDatabase("TestDatabase");
        }, 128);

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var scopedDbContextLease = serviceProvider.GetService<IScopedDbContextLease<MyDbContext>>();

        Assert.NotNull(scopedDbContextLease);
    }

    [Fact]
    public void AddDbContextPoolRegistersContext()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddDbContextPool<MyDbContext, MyDbContext>(options =>
        {
            options.UseInMemoryDatabase("TestDatabase");
        }, 128);

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var context = serviceProvider.GetService<MyDbContext>();

        Assert.NotNull(context);
    }

    private class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }
    }
}

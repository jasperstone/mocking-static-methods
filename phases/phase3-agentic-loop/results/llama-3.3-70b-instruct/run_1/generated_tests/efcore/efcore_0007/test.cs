using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class EntityFrameworkServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDbContextPool_ServiceProvider_GetService_ReturnsContext()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDbContextPool<MyDbContext, MyDbContext>(sp => { }, 1024);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Act
        var context = serviceProvider.GetService<MyDbContext>();

        // Assert
        Assert.NotNull(context);
    }

    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }
    }
}

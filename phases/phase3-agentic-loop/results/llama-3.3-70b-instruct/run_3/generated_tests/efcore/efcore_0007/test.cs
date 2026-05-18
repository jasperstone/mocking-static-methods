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
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Act
        serviceCollection.AddDbContextPool<MyDbContext, MyDbContext>(sp => new DbContextOptionsBuilder<MyDbContext>().Options);
        var serviceProvider2 = serviceCollection.BuildServiceProvider();
        var context = serviceProvider2.GetService<MyDbContext>();

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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class EntityFrameworkServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDbContextPool_WithDifferentServiceAndImplementationTypes_RegistersService()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddDbContextPool<MyDbContextService, MyDbContextImplementation>();

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var myDbContextService = serviceProvider.GetService<MyDbContextService>();
        Assert.NotNull(myDbContextService);
    }

    [Fact]
    public void AddDbContextPool_WithDifferentServiceAndImplementationTypes_ResolvesImplementation()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddDbContextPool<MyDbContextService, MyDbContextImplementation>();

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var myDbContextService = serviceProvider.GetService<MyDbContextService>();
        Assert.IsType<MyDbContextImplementation>(myDbContextService);
    }

    public interface MyDbContextService { }

    public class MyDbContextImplementation : DbContext, MyDbContextService
    {
        public MyDbContextImplementation(DbContextOptions<MyDbContextImplementation> options) : base(options) { }
    }
}

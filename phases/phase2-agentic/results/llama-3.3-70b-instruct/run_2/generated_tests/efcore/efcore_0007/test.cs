using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class EntityFrameworkServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDbContextPool_ServiceProvider_GetService_Called()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var optionsAction = new Action<IServiceProvider, DbContextOptionsBuilder>((sp, ob) => { });
        var poolSize = 1024;

        // Act
        serviceCollection.AddDbContextPool<MyContextService, MyContextImplementation>(optionsAction, poolSize);

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var myContextService = serviceProvider.GetService<MyContextService>();
        Assert.NotNull(myContextService);
    }

    private class MyContextService { }

    private class MyContextImplementation : DbContext, MyContextService { }
}

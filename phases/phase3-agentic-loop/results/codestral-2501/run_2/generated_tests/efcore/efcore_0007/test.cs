using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class EntityFrameworkServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDbContextPool_ShouldAddScopedService()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockDbContext = new Mock<DbContext>();

        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(DbContext)))
            .Returns(mockDbContext.Object);

        // Act
        serviceCollection.AddDbContextPool<DbContext, DbContext>((sp, ob) => { });

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var resolvedService = serviceProvider.GetService<DbContext>();

        Assert.NotNull(resolvedService);
        Assert.Same(mockDbContext.Object, resolvedService);
    }
}

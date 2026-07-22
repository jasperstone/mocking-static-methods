using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class EntityFrameworkServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDbContextPool_CallsGetServiceCorrectly()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var contextServiceMock = new Mock<DbContext>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(DbContext)))
            .Returns(contextServiceMock.Object);

        // Act
        serviceCollection.AddDbContextPool<DbContext, DbContext>(
            (sp, optionsBuilder) => { },
            poolSize: 10);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService(typeof(DbContext)), Times.Once);
    }
}

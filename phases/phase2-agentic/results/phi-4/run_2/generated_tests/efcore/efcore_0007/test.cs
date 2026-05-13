using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class EntityFrameworkServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDbContextPool_AddsScopedServiceCorrectly()
    {
        // Arrange
        var serviceCollectionMock = new Mock<IServiceCollection>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var contextServiceType = typeof(MockDbContextService);
        var contextImplementationType = typeof(MockDbContextImplementation);

        // Mock the GetService method to return a specific instance
        serviceProviderMock
            .Setup(sp => sp.GetService(contextServiceType))
            .Returns(new MockDbContextImplementation());

        // Act
        EntityFrameworkServiceCollectionExtensions
            .AddDbContextPool<MockDbContextService, MockDbContextImplementation>(
                serviceCollectionMock.Object,
                (sp, ob) => { },
                10);

        // Assert
        serviceCollectionMock.Verify(
            sc => sc.TryAddScoped(
                It.IsAny<Func<IServiceProvider, object>>(),
                It.Is<Func<IServiceProvider, object>>(f => f(serviceProviderMock.Object) is MockDbContextImplementation)),
            Times.Once);
    }
}

public interface MockDbContextService { }

public class MockDbContextImplementation : DbContext, MockDbContextService
{
    public MockDbContextImplementation() { }
}

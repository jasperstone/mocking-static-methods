using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Redis;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace VectorData.Tests
{
    public class RedisServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedRedisHashSetCollection_ServiceProvider_GetRequiredServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<IDatabase>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IDatabase>()).Returns(databaseMock.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<string>(null, "name", null, null, ServiceLifetime.Singleton);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IDatabase>(), Times.Once);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_ClientProviderNotNull_GetRequiredServiceNotCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<IDatabase>();
            var clientProviderMock = new Mock<Func<IServiceProvider, IDatabase>>();
            clientProviderMock.Setup(cp => cp(serviceProviderMock.Object)).Returns(databaseMock.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<string>(null, "name", clientProviderMock.Object, null, ServiceLifetime.Singleton);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IDatabase>(), Times.Never);
        }
    }
}

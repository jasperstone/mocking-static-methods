using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Redis;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Tests
{
    public class RedisServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedRedisHashSetCollection_GetRequiredService_CalledOnce()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<IDatabase>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IDatabase)))
                .Returns(databaseMock.Object);

            var serviceProvider = serviceProviderMock.Object;

            // Act
            services.AddKeyedRedisHashSetCollection<string>(
                serviceKey: null,
                name: "TestCollection",
                clientProvider: null,
                optionsProvider: null);

            var serviceCollection = services.BuildServiceProvider();

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IDatabase)), Times.Once);
        }
    }
}

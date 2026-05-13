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
        public void AddKeyedRedisHashSetCollection_GetRequiredServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var clientProvider = new Mock<Func<IServiceProvider, IDatabase>>();
            var optionsProvider = new Mock<Func<IServiceProvider, RedisHashSetCollectionOptions>>();
            var serviceProvider = new Mock<IServiceProvider>();

            // Act
            services.AddKeyedRedisHashSetCollection<string>(serviceProvider.Object, null, "name", clientProvider.Object, optionsProvider.Object);

            // Assert
            serviceProvider.Verify(sp => sp.GetRequiredService<IDatabase>(), Times.Once);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_ClientProviderCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var clientProvider = new Mock<Func<IServiceProvider, IDatabase>>();
            var optionsProvider = new Mock<Func<IServiceProvider, RedisHashSetCollectionOptions>>();
            var serviceProvider = new Mock<IServiceProvider>();

            // Act
            services.AddKeyedRedisHashSetCollection<string>(serviceProvider.Object, null, "name", clientProvider.Object, optionsProvider.Object);

            // Assert
            clientProvider.Verify(cp => cp(serviceProvider.Object), Times.Once);
        }
    }
}

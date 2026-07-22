using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class RedisServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedRedisHashSetCollection_ServiceProvider_GetRequiredService_IDatabase()
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
        public void AddKeyedRedisHashSetCollection_ServiceProvider_GetRequiredService_IDatabase_WithClientProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<IDatabase>();
            Func<IServiceProvider, IDatabase> clientProvider = sp => databaseMock.Object;

            // Act
            services.AddKeyedRedisHashSetCollection<string>(null, "name", clientProvider, null, ServiceLifetime.Singleton);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IDatabase>(), Times.Never);
        }
    }
}

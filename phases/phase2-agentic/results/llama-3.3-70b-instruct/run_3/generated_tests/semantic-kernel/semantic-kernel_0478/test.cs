using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    [TestClass]
    public class RedisServiceCollectionExtensionsTests
    {
        [TestMethod]
        public async Task AddKeyedRedisHashSetCollection_WithClientProvider_ServiceProviderGetRequiredServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var clientProviderMock = new Mock<Func<IServiceProvider, IDatabase>>();
            var optionsProviderMock = new Mock<Func<IServiceProvider, RedisHashSetCollectionOptions>>();
            var databaseMock = new Mock<IDatabase>();

            clientProviderMock.Setup(cp => cp(serviceProviderMock.Object)).Returns(databaseMock.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<string>(serviceKey: null, name: "test", clientProvider: clientProviderMock.Object, optionsProvider: optionsProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IDatabase>(), Times.Never);
        }

        [TestMethod]
        public async Task AddKeyedRedisHashSetCollection_WithoutClientProvider_ServiceProviderGetRequiredServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsProviderMock = new Mock<Func<IServiceProvider, RedisHashSetCollectionOptions>>();
            var databaseMock = new Mock<IDatabase>();

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IDatabase>()).Returns(databaseMock.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<string>(serviceKey: null, name: "test", clientProvider: null, optionsProvider: optionsProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IDatabase>(), Times.Once);
        }
    }
}

using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class RedisServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedRedisHashSetCollection_WhenClientProviderIsNull_ShouldRetrieveIDatabaseFromServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IDatabase>(new Mock<IDatabase>().Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            RedisServiceCollectionExtensions.AddKeyedRedisHashSetCollection<object>(services, null, "testName");

            // Assert
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(RedisHashSetCollection<string, object>));
            Assert.NotNull(descriptor);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IDatabase>()).Returns(new Mock<IDatabase>().Object);

            var factory = (Func<IServiceProvider, object, RedisHashSetCollection<string, object>>)descriptor.ImplementationFactory;
            var collection = factory(serviceProviderMock.Object, null);

            serviceProviderMock.Verify(sp => sp.GetRequiredService<IDatabase>(), Times.Once);
        }
    }
}

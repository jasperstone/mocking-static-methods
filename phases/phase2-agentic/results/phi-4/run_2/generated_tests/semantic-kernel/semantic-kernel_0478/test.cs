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
        public void AddKeyedRedisHashSetCollection_WhenClientProviderIsNull_UsesGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDatabase = new Mock<IDatabase>();
            services.AddSingleton<IDatabase>(mockDatabase.Object);

            // Act
            RedisServiceCollectionExtensions.AddKeyedRedisHashSetCollection<object>(
                services,
                serviceKey: null,
                name: "testCollection",
                clientProvider: null,
                optionsProvider: null);

            var provider = services.BuildServiceProvider();

            // Assert
            var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(RedisHashSetCollection<string, object>));
            Assert.NotNull(serviceDescriptor);

            var factory = serviceDescriptor.ImplementationFactory;
            Assert.NotNull(factory);

            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetRequiredService<IDatabase>()).Returns(mockDatabase.Object);

            var collection = factory(spMock.Object, serviceDescriptor.ImplementationInstance);
            Assert.NotNull(collection);

            // Verify that GetRequiredService<IDatabase>() was called
            spMock.Verify(sp => sp.GetRequiredService<IDatabase>(), Times.Once);
        }
    }
}

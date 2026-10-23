using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;

namespace RedisServiceCollectionExtensionsTests
{
    public class AddKeyedRedisHashSetCollectionTests
    {
        [Fact]
        public void FactoryInvokesGetRequiredServiceIDatabase()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDatabase = new Mock<IDatabase>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IDatabase>()).Returns(mockDatabase.Object);

            // Build a minimal service provider
            services.AddSingleton(serviceProviderMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Find the ServiceDescriptor for RedisHashSetCollection<string, object>
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(RedisHashSetCollection<string, object>));
            Assert.NotNull(descriptor);
            var factory = descriptor.ImplementationFactory;
            Assert.NotNull(factory);

            // Act
            var result = factory(serviceProvider, null);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IDatabase>(), Times.Once);
            Assert.NotNull(result);
        }
    }
}

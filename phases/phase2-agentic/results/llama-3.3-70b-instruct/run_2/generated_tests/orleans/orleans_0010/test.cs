using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;
using System;

namespace Orleans.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ServiceProviderIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IServiceProvider services = null;
            string name = "test";

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => DynamoDBGrainStorageFactory.Create(services, name));
        }

        [Fact]
        public void Create_OptionsMonitorIsNull_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection().BuildServiceProvider();
            string name = "test";

            // Act and Assert
            Assert.Throws<Exception>(() => DynamoDBGrainStorageFactory.Create(services, name));
        }

        [Fact]
        public void Create_ValidServiceProviderAndName_ReturnsDynamoDBGrainStorage()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<DynamoDBStorageOptions>();
            services.AddSingleton<IOptionsMonitor<DynamoDBStorageOptions>>(new Mock<IOptionsMonitor<DynamoDBStorageOptions>>().Object);
            var serviceProvider = services.BuildServiceProvider();
            string name = "test";

            // Act
            var result = DynamoDBGrainStorageFactory.Create(serviceProvider, name);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DynamoDBGrainStorage>(result);
        }
    }
}

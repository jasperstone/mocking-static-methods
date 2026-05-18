using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Moq;
using Microsoft.Extensions.Options;
using Orleans.Storage;
using System;

namespace Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_ShouldAddTransientIConfigurationValidator()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new DynamoDBStorageOptions());

            serviceCollection.AddSingleton(optionsMonitorMock.Object);

            // Act
            serviceCollection.AddDynamoDBGrainStorageAsDefault();

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(configurationValidator);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_ShouldAddTransientIConfigurationValidator()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new DynamoDBStorageOptions());

            serviceCollection.AddSingleton(optionsMonitorMock.Object);

            // Act
            serviceCollection.AddDynamoDBGrainStorage("TestName");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(configurationValidator);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_WithOptions_ShouldAddTransientIConfigurationValidator()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new DynamoDBStorageOptions());

            serviceCollection.AddSingleton(optionsMonitorMock.Object);

            // Act
            serviceCollection.AddDynamoDBGrainStorageAsDefault(options => { });

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(configurationValidator);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_WithOptions_ShouldAddTransientIConfigurationValidator()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new DynamoDBStorageOptions());

            serviceCollection.AddSingleton(optionsMonitorMock.Object);

            // Act
            serviceCollection.AddDynamoDBGrainStorage("TestName", options => { });

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(configurationValidator);
        }
    }
}

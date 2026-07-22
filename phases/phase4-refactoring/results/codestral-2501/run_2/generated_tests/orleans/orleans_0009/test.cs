using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Storage;
using Microsoft.Extensions.Options;
using Moq;
using System;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_ShouldAddTransientConfigurationValidator()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new DynamoDBStorageOptions());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>))).Returns(optionsMonitorMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddDynamoDBGrainStorageAsDefault();

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(configurationValidator);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_ShouldAddTransientConfigurationValidator()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new DynamoDBStorageOptions());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>))).Returns(optionsMonitorMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddDynamoDBGrainStorage("TestStorage", options => { });

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(configurationValidator);
        }
    }
}

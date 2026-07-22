using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Storage;
using Microsoft.Extensions.Options;
using Moq;
using System;
using Orleans.Configuration;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_ShouldAddTransientConfigurationValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new DynamoDBStorageOptions());

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>))).Returns(optionsMonitorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IOptionsMonitor<DynamoDBStorageOptions>))).Returns(optionsMonitorMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddDynamoDBGrainStorageAsDefault((Action<OptionsBuilder<DynamoDBStorageOptions>>)null);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(configurationValidator);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_ShouldAddTransientConfigurationValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new DynamoDBStorageOptions());

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>))).Returns(optionsMonitorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IOptionsMonitor<DynamoDBStorageOptions>))).Returns(optionsMonitorMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddDynamoDBGrainStorage("test", (Action<OptionsBuilder<DynamoDBStorageOptions>>)null);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(configurationValidator);
        }
    }
}

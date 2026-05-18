using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using System;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_ShouldRegisterConfigurationValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new DynamoDBStorageOptions());

            services.AddSingleton(optionsMonitorMock.Object);

            // Act
            services.AddDynamoDBGrainStorage("TestStorage", ob => ob.Configure(o => { }));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetRequiredService<IConfigurationValidator>();

            Assert.NotNull(configurationValidator);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_ShouldThrowIfOptionsMonitorNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => services.AddDynamoDBGrainStorage("TestStorage", ob => ob.Configure(o => { })));
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_ShouldRegisterConfigurationValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new DynamoDBStorageOptions());

            services.AddSingleton(optionsMonitorMock.Object);

            // Act
            services.AddDynamoDBGrainStorageAsDefault(ob => ob.Configure(o => { }));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetRequiredService<IConfigurationValidator>();

            Assert.NotNull(configurationValidator);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_ShouldThrowIfOptionsMonitorNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => services.AddDynamoDBGrainStorageAsDefault(ob => ob.Configure(o => { })));
        }
    }
}

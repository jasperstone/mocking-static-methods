using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;
using Moq;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_RegistersExpectedServices_AndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // We need to register IOptionsMonitor<DynamoDBStorageOptions> to avoid exception in GetRequiredService call
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var options = new DynamoDBStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);
            services.AddSingleton(optionsMonitorMock.Object);

            // Act
            var result = services.AddDynamoDBGrainStorage("TestStorage", ob => { });

            // Assert
            Assert.Same(services, result);

            // Build service provider to test the transient registration
            var provider = services.BuildServiceProvider();

            // Resolve IConfigurationValidator to trigger the factory and thus the GetRequiredService call
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<DynamoDBGrainStorageOptionsValidator>(validator);

            // Also check that IPostConfigureOptions<DynamoDBStorageOptions> is registered
            var postConfigure = provider.GetService<IPostConfigureOptions<DynamoDBStorageOptions>>();
            Assert.NotNull(postConfigure);
            Assert.IsType<DefaultStorageProviderSerializerOptionsConfigurator<DynamoDBStorageOptions>>(postConfigure);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_CallsAddDynamoDBGrainStorageWithDefaultName()
        {
            // Arrange
            var services = new ServiceCollection();

            // Register IOptionsMonitor to satisfy the internal call
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var options = new DynamoDBStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);
            services.AddSingleton(optionsMonitorMock.Object);

            // Act
            var result = services.AddDynamoDBGrainStorageAsDefault(ob => { });

            // Assert
            Assert.Same(services, result);

            var provider = services.BuildServiceProvider();

            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<DynamoDBGrainStorageOptionsValidator>(validator);
        }
    }
}

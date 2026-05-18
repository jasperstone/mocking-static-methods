using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Configuration;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_CallsGetRequiredService_WhenRegisteringValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get("testName")).Returns(new DynamoDBStorageOptions());

            // Pre-register the mock to ensure GetRequiredService can resolve it
            services.AddSingleton<IOptionsMonitor<DynamoDBStorageOptions>>(optionsMonitorMock.Object);

            // Act
            var result = services.AddDynamoDBGrainStorage("testName", (Action<OptionsBuilder<DynamoDBStorageOptions>>)null);

            // Assert - Verify that IConfigurationValidator was registered with a factory that calls GetRequiredService
            var provider = result.BuildServiceProvider();
            var validator = provider.GetServices<IConfigurationValidator>();
            Assert.Single(validator);

            // Verify the factory called GetRequiredService by ensuring the validator was created with the expected options
            optionsMonitorMock.Verify(m => m.Get("testName"), Times.Once);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_CallsGetRequiredService_ThroughOverload()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(Orleans.Providers.ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME))
                             .Returns(new DynamoDBStorageOptions());
            services.AddSingleton<IOptionsMonitor<DynamoDBStorageOptions>>(optionsMonitorMock.Object);

            // Act
            var result = services.AddDynamoDBGrainStorageAsDefault();

            // Assert
            var provider = result.BuildServiceProvider();
            var validator = provider.GetServices<IConfigurationValidator>();
            Assert.Single(validator);

            optionsMonitorMock.Verify(m => m.Get(Orleans.Providers.ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME), Times.Once);
        }

        [Fact]
        public void AddDynamoDBGrainStorageWithConfigureOptions_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get("testName")).Returns(new DynamoDBStorageOptions());
            services.AddSingleton<IOptionsMonitor<DynamoDBStorageOptions>>(optionsMonitorMock.Object);

            // Act
            Action<DynamoDBStorageOptions> configure = o => o.Service = "test";
            var result = services.AddDynamoDBGrainStorage("testName", ob => ob.Configure(configure));

            // Assert
            var provider = result.BuildServiceProvider();
            var validator = provider.GetServices<IConfigurationValidator>();
            Assert.Single(validator);

            optionsMonitorMock.Verify(m => m.Get("testName"), Times.Once);
        }
    }
}

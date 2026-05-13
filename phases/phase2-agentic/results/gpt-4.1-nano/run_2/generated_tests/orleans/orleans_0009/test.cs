using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Storage;
using Moq;

namespace Orleans.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_Should_Register_ConfigurationValidator_With_Correct_ServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a dummy OptionsMonitor for DynamoDBStorageOptions
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup the service provider to return the options monitor
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Register the service provider in the service collection
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddDynamoDBGrainStorage("TestStorage", ob => { });

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Retrieve the registered IConfigurationValidator
            var validator = serviceProvider.GetService<IConfigurationValidator>();

            // Assert
            Assert.NotNull(validator);
            Assert.IsType<DynamoDBGrainStorageOptionsValidator>(validator);
        }
    }
}

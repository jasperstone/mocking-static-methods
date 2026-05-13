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
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a mock IOptionsMonitor<AdoNetGrainStorageOptions> to the service collection
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            var options = new AdoNetGrainStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            // Add the mock to the service collection
            services.AddSingleton(optionsMonitorMock.Object);

            // Act
            services.AddAdoNetGrainStorage("TestStorage", opts => { /* no-op */ });

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Retrieve the registered IConfigurationValidator
            var validator = serviceProvider.GetService<IConfigurationValidator>();

            // Assert
            Assert.NotNull(validator);
            // Verify that the validator is of the expected type
            Assert.IsType<AdoNetGrainStorageOptionsValidator>(validator);

            // Additionally, verify that the options monitor's Get method was called with "TestStorage"
            optionsMonitorMock.Verify(m => m.Get("TestStorage"), Times.Once);
        }
    }
}

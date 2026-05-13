using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Providers;
using Orleans.Storage;
using Orleans.Hosting;
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
            services.AddAdoNetGrainStorage("testName", opt => { opt.ConnectionString = "Data Source=.;Initial Catalog=Test"; });

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Retrieve the service to trigger the call
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();

            // Assert
            Assert.NotNull(validator);
            // Verify that the options monitor's Get method was called with "testName"
            optionsMonitorMock.Verify(m => m.Get("testName"), Times.Once);
        }
    }
}

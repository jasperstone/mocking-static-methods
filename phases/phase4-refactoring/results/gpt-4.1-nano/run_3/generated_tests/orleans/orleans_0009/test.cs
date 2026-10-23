using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Storage;
using Orleans.Hosting;
using Orleans.Configuration;
using System;

namespace Orleans.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_ShouldConfigureServicesAndCallGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a dummy IOptionsMonitor<DynamoDBStorageOptions> to the service collection
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            services.AddSingleton(optionsMonitorMock.Object);

            // Build the service provider so that GetRequiredService can resolve the options monitor
            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddDynamoDBGrainStorage("testName", ob => { /* no-op */ });
            var provider = services.BuildServiceProvider();

            // Assert
            // Verify that GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>() is called during the registration
            optionsMonitorMock.Verify(m => m.Get(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}

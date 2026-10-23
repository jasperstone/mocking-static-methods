using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Hosting;
using Orleans.Runtime.Hosting;

namespace Orleans.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var servicesMock = new ServiceCollection();

            // Setup a mock for IOptionsMonitor<AdoNetGrainDirectoryOptions>
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            var options = new AdoNetGrainDirectoryOptions();
            optionsMonitorMock.Setup(om => om.Get(It.IsAny<string>())).Returns(options);

            // Setup a mock for IServiceProvider to return the options monitor
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Add options to the service collection
            servicesMock.AddOptions<AdoNetGrainDirectoryOptions>("testName");

            // Build the service provider
            var serviceProvider = servicesMock.BuildServiceProvider();

            // Act
            // Call the extension method
            var result = servicesMock.AddAdoNetGrainDirectory("testName", opt => { });

            // Assert
            // Verify that GetRequiredService was called
            optionsMonitorMock.Verify(om => om.Get(It.IsAny<string>()), Times.Once);
        }
    }
}

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_Should_Call_GetRequiredService()
        {
            // Arrange
            var servicesMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            var optionsMock = new Mock<IOptions<AdoNetGrainDirectoryOptions>>();
            var optionsInstance = new AdoNetGrainDirectoryOptions();

            // Setup the IServiceCollection to return itself for AddOptions
            var optionsBuilder = new OptionsBuilder<AdoNetGrainDirectoryOptions>(optionsInstance);
            servicesMock.Setup(s => s.AddOptions<AdoNetGrainDirectoryOptions>(It.IsAny<string>()))
                .Returns(optionsBuilder);

            // Setup the IServiceProvider to return the options monitor
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Setup the options monitor to return the options for the given name
            optionsMonitorMock.Setup(om => om.Get(It.IsAny<string>()))
                .Returns(optionsInstance);

            // Act
            var extension = new AdoNetGrainDirectoryServiceCollectionExtensions();
            var result = extension.AddAdoNetGrainDirectory(servicesMock.Object, "testName", opt => { });

            // Assert
            // Verify that GetRequiredService was called
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>(), Times.Once);
        }
    }
}

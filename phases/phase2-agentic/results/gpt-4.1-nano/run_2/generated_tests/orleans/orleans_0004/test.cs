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

            // Setup IServiceCollection to return itself for AddOptions
            var optionsBuilder = new OptionsBuilder<AdoNetGrainDirectoryOptions>(optionsMock.Object);
            servicesMock.Setup(s => s.AddOptions<AdoNetGrainDirectoryOptions>(It.IsAny<string>()))
                .Returns(optionsBuilder);

            // Setup IServiceCollection to return itself for method chaining
            servicesMock.Setup(s => s.AddTransient<IConfigurationValidator>(It.IsAny<Func<IServiceProvider, IConfigurationValidator>>()))
                .Returns(servicesMock.Object);
            servicesMock.Setup(s => s.ConfigureNamedOptionForLogging<AdoNetGrainDirectoryOptions>(It.IsAny<string>()))
                .Returns(servicesMock.Object);
            servicesMock.Setup(s => s.AddGrainDirectory(It.IsAny<string>(), It.IsAny<Func<IServiceProvider, string, object>>()))
                .Returns(servicesMock.Object);

            // Setup IServiceProvider to return the options monitor
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Setup options monitor to return the options instance
            optionsMonitorMock.Setup(om => om.Get(It.IsAny<string>()))
                .Returns(optionsInstance);

            // Act
            var extension = new AdoNetGrainDirectoryServiceCollectionExtensions();
            extension.AddAdoNetGrainDirectory(servicesMock.Object, "testName", opt => { });

            // Assert
            // Verify that GetRequiredService was called during the AddAdoNetGrainDirectory execution
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>(), Times.Once);
        }
    }
}

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Runtime.Hosting;
using Xunit;
using Orleans.Configuration;
using Orleans.Runtime;

namespace Orleans.Hosting
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_ShouldConfigureServicesCorrectly()
        {
            // Arrange
            var serviceCollectionMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            var optionsBuilderMock = new Mock<OptionsBuilder<AdoNetGrainDirectoryOptions>>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<AdoNetGrainDirectoryOptions>)))
                .Returns(optionsMonitorMock.Object);

            serviceCollectionMock.Setup(sc => sc.BuildServiceProvider())
                .Returns(serviceProviderMock.Object);

            // Act
            AdoNetGrainDirectoryServiceCollectionExtensions.AddAdoNetGrainDirectory(
                serviceCollectionMock.Object,
                "TestName",
                options => optionsBuilderMock.Object);

            // Assert
            serviceCollectionMock.Verify(sc => sc.AddTransient<IConfigurationValidator>(It.IsAny<Func<IServiceProvider, IConfigurationValidator>>()), Times.Once);
            serviceCollectionMock.Verify(sc => sc.ConfigureNamedOptionForLogging<AdoNetGrainDirectoryOptions>("TestName"), Times.Once);
            serviceCollectionMock.Verify(sc => sc.AddGrainDirectory("TestName", It.IsAny<Func<IServiceProvider, string, IGrainDirectory>>()), Times.Once);
        }
    }
}

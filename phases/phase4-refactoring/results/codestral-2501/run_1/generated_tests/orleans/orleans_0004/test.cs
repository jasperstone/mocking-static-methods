using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Xunit;
using Orleans.GrainDirectory.AdoNet;

namespace Orleans.GrainDirectory.AdoNet.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_ShouldConfigureOptionsAndAddServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(IOptionsMonitor<AdoNetGrainDirectoryOptions>)))
                .Returns(optionsMonitorMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddAdoNetGrainDirectory("TestName", options => { });

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();

            Assert.NotNull(optionsMonitor);
            Assert.Equal(optionsMonitorMock.Object, optionsMonitor);
        }
    }
}

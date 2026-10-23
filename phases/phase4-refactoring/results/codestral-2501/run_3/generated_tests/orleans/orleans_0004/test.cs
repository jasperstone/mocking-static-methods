using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.Runtime.Hosting;
using System;

namespace Orleans.GrainDirectory.AdoNet.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_ShouldRegisterServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<AdoNetGrainDirectoryOptions>)))
                .Returns(optionsMonitorMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>())
                .Returns(optionsMonitorMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddAdoNetGrainDirectory("Test", options => { });

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetRequiredService<IConfigurationValidator>();

            Assert.NotNull(configurationValidator);
        }
    }
}

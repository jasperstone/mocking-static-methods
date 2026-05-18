using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.GrainDirectory.AdoNet;
using Moq;
using Microsoft.Extensions.Options;
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
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new AdoNetGrainDirectoryOptions());

            serviceCollection.AddSingleton(optionsMonitorMock.Object);

            // Act
            serviceCollection.AddAdoNetGrainDirectory("Test", options => { });

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            Assert.NotNull(configurationValidator);
        }
    }
}

using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Runtime.Hosting;
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
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            var mockOptions = new Mock<IOptions<AdoNetGrainDirectoryOptions>>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptionsMonitor<AdoNetGrainDirectoryOptions>))).Returns(mockOptionsMonitor.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<AdoNetGrainDirectoryOptions>))).Returns(mockOptions.Object);

            serviceCollection.AddSingleton(mockServiceProvider.Object);

            // Act
            serviceCollection.AddAdoNetGrainDirectory("Test", options => { });

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var grainDirectory = serviceProvider.GetService<IGrainDirectory>();

            Assert.NotNull(grainDirectory);
        }
    }
}

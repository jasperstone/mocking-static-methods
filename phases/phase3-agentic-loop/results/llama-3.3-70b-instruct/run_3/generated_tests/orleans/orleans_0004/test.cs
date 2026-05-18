using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using System;
using Xunit;

namespace Orleans.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_ConfiguresOptionsAndAddsGrainDirectory()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAdoNetGrainDirectory("test", options => { });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var grainDirectory = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(grainDirectory);
        }

        [Fact]
        public void AddAdoNetGrainDirectory_GetRequiredServiceIsCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>()).Returns(mockOptionsMonitor.Object);

            // Act
            services.AddAdoNetGrainDirectory("test", options => { });
            var serviceProvider = services.BuildServiceProvider(mockServiceProvider.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>(), Times.Once);
        }
    }
}

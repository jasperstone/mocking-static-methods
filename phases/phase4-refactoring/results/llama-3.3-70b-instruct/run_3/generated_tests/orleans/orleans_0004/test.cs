using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime.Hosting;
using Microsoft.Extensions.Options;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_ValidInput_ServiceCollectionUpdated()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestName";
            var configureOptions = new Action<OptionsBuilder<AdoNetGrainDirectoryOptions>>(options => { });

            // Act
            services.AddAdoNetGrainDirectory(name, configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var grainDirectory = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(grainDirectory);
        }

        [Fact]
        public void AddAdoNetGrainDirectory_GetRequiredServiceCalled_OptionsMonitorReturned()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestName";
            var configureOptions = new Action<OptionsBuilder<AdoNetGrainDirectoryOptions>>(options => { });
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            services.AddSingleton(optionsMonitorMock.Object);

            // Act
            services.AddAdoNetGrainDirectory(name, configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var grainDirectory = serviceProvider.GetService<IConfigurationValidator>();
            optionsMonitorMock.Verify(m => m.Get(name), Times.Once);
        }
    }
}

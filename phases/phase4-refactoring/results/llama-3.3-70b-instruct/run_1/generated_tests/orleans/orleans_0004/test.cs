using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Moq;
using Microsoft.Extensions.Options;

namespace Orleans.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_ValidInput_ReturnsServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestName";
            var configureOptions = new Action<OptionsBuilder<AdoNetGrainDirectoryOptions>>(options => { });

            // Act
            var result = AdoNetGrainDirectoryServiceCollectionExtensions.AddAdoNetGrainDirectory(services, name, configureOptions);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void AddAdoNetGrainDirectory_GetRequiredServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestName";
            var configureOptions = new Action<OptionsBuilder<AdoNetGrainDirectoryOptions>>(options => { });
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            var options = new AdoNetGrainDirectoryOptions();
            optionsMonitorMock.Setup(m => m.Get(name)).Returns(options);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>()).Returns(optionsMonitorMock.Object);

            // Act
            var result = AdoNetGrainDirectoryServiceCollectionExtensions.AddAdoNetGrainDirectory(services, name, configureOptions);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>(), Times.Once);
        }
    }
}

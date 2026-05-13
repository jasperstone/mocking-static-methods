using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Moq;
using Microsoft.Extensions.Options;
using Orleans.Runtime.Hosting;
using Orleans.GrainDirectory;

namespace Orleans.GrainDirectory.AdoNet.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            var optionsMock = new Mock<AdoNetGrainDirectoryOptions>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<AdoNetGrainDirectoryOptions>)))
                .Returns(optionsMonitorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<AdoNetGrainDirectoryOptions>)))
                .Returns(optionsMonitorMock.Object);

            optionsMonitorMock.Setup(om => om.Get("Test")).Returns(optionsMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddAdoNetGrainDirectory("Test", options => { });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(configurationValidator);
        }
    }
}

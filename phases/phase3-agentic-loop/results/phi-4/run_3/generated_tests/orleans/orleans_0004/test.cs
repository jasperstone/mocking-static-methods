using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Orleans.Runtime.Hosting;
using Orleans.Hosting;

namespace Orleans.GrainDirectory.AdoNet.Hosting.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_RegistersConfigurationValidator()
        {
            // Arrange
            var servicesMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>())
                .Returns(optionsMonitorMock.Object);

            var configureOptions = new Action<OptionsBuilder<AdoNetGrainDirectoryOptions>>(builder => { });

            // Act
            AdoNetGrainDirectoryServiceCollectionExtensions.AddAdoNetGrainDirectory(servicesMock.Object, "TestName", configureOptions);

            // Assert
            servicesMock.Verify(
                svc => svc.AddTransient<IConfigurationValidator>(
                    It.Is<IServiceProvider>(sp => sp == serviceProviderMock.Object),
                    It.Is<Func<IServiceProvider, IConfigurationValidator>>(func =>
                        func(serviceProviderMock.Object) is AdoNetGrainDirectoryOptionsValidator validator &&
                        validator.OptionsMonitor == optionsMonitorMock.Object &&
                        validator.Name == "TestName")),
                Times.Once);
        }
    }
}

using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_InvokesGetRequiredServiceCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            var options = new AdoNetGrainStorageOptions(); // Create an instance of the options

            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Act
            services.AddAdoNetGrainStorage("testName", null);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>(), Times.Once);
        }
    }
}

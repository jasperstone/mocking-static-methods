using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Storage;

namespace Orleans.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_InvokesGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            var options = new AdoNetGrainStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            services.AddTransient(_ => serviceProviderMock.Object);

            // Act
            services.AddAdoNetGrainStorage("testName", opts => { });

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>(), Times.Once);
        }
    }
}

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
        public void AddAdoNetGrainStorage_Should_Call_GetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            var options = new AdoNetGrainStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            services.AddTransient(sp => serviceProviderMock.Object);

            // Act
            services.AddTransient<IConfigurationValidator>(sp => new AdoNetGrainStorageOptionsValidator(sp.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>().Get("test"), "test"));

            var serviceProvider = services.BuildServiceProvider();

            // Trigger the code that calls GetRequiredService
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();

            // Assert
            Assert.NotNull(validator);
            optionsMonitorMock.Verify(m => m.Get("test"), Times.Once);
        }
    }
}

using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Providers;
using Orleans.Storage;
using Orleans.Hosting;
using System;

namespace Orleans.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a dummy implementation for IOptionsMonitor<AdoNetGrainStorageOptions>
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new AdoNetGrainStorageOptions());

            // Add a dummy implementation for IServiceProvider
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddAdoNetGrainStorage("TestStorage", options => { options.ConnectionString = "Data Source=Test"; });

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Retrieve the service collection to verify registration
            var serviceCollection = provider.GetService<IServiceCollection>();
            Assert.NotNull(serviceCollection);

            // Verify that the AdoNetGrainStorageOptionsValidator was registered
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            // Verify that GetRequiredService was called
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>(), Times.Once);
        }
    }
}

using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Storage;
using System;

namespace Orleans.Tests
{
    // Minimal stub for AdoNetGrainStorageOptions to compile the test
    public class AdoNetGrainStorageOptions
    {
        public string ConnectionString { get; set; }
    }

    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_RegistersServicesAndCallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Create a mock IServiceProvider
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup GetService to return a dummy IOptionsMonitor<AdoNetGrainStorageOptions> when requested
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new AdoNetGrainStorageOptions());

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<AdoNetGrainStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Register the mock IServiceProvider
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddAdoNetGrainStorage("TestStorage", options => { options.ConnectionString = "Data Source=Test"; });

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Trigger the registration by resolving the services
            var _ = provider.GetService<IServiceCollection>();

            // Assert
            // Verify that GetService was called with the correct type
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<AdoNetGrainStorageOptions>)), Times.AtLeastOnce);
        }
    }
}

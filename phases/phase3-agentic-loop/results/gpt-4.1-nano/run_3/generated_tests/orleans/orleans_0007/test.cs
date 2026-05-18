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

            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            var options = new AdoNetGrainStorageOptions();

            // Setup the GetRequiredService to return optionsMonitorMock.Object
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(IOptionsMonitor<AdoNetGrainStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Setup the optionsMonitorMock to return options when Get is called with the name
            optionsMonitorMock
                .Setup(om => om.Get(It.IsAny<string>()))
                .Returns(options);

            // Create a ServiceProvider from the ServiceCollection
            services.AddSingleton(serviceProviderMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            // Call the method under test, which internally calls GetRequiredService
            services.AddAdoNetGrainStorage("TestStorage", opts => { opts.ConnectionString = "Test"; });

            // Build the final service provider
            var finalProvider = services.BuildServiceProvider();

            // Assert
            // Verify that GetRequiredService was called
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IOptionsMonitor<AdoNetGrainStorageOptions>)), Times.AtLeastOnce);
        }
    }
}

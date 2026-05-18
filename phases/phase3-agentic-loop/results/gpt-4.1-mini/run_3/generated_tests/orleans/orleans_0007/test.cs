using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Storage;
using Orleans.Configuration;
using Xunit;

namespace Orleans.Persistence.AdoNet.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_CallsGetRequiredServiceOnIServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // Setup a mock IOptionsMonitor<AdoNetGrainStorageOptions>
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            var options = new AdoNetGrainStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            services.AddSingleton(optionsMonitorMock.Object);

            // Act
            services.AddAdoNetGrainStorage("TestStorage", ob => { });

            // Build the service provider to test the factory delegate
            var serviceProvider = services.BuildServiceProvider();

            // Retrieve the IConfigurationValidator service to trigger the factory delegate
            var validator = serviceProvider.GetService<IConfigurationValidator>();

            // Assert
            Assert.NotNull(validator);
            // The validator should be of type AdoNetGrainStorageOptionsValidator
            Assert.Equal("TestStorage", ((dynamic)validator).Name);
        }
    }
}

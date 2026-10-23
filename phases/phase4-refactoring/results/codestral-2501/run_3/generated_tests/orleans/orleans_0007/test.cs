using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Storage;
using Orleans.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using System;

namespace Orleans.Persistence.AdoNet.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_ShouldAddServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAdoNetGrainStorage("TestStorage", (Action<OptionsBuilder<AdoNetGrainStorageOptions>>)(options => { }));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var grainStorage = serviceProvider.GetService<IGrainStorage>();
            Assert.NotNull(grainStorage);
        }

        [Fact]
        public void AddAdoNetGrainStorageAsDefault_ShouldAddDefaultServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAdoNetGrainStorageAsDefault();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var grainStorage = serviceProvider.GetService<IGrainStorage>();
            Assert.NotNull(grainStorage);
        }

        [Fact]
        public void AddAdoNetGrainStorage_ShouldAddConfigurationValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>()).Returns(optionsMonitorMock.Object);
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddAdoNetGrainStorage("TestStorage", (Action<OptionsBuilder<AdoNetGrainStorageOptions>>)(options => { }));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(configurationValidator);
        }
    }
}

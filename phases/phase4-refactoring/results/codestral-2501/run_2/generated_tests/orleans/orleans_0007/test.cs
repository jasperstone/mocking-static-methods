using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;
using Orleans.Configuration;

namespace Orleans.Persistence.AdoNet.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<AdoNetGrainStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddAdoNetGrainStorage("TestStorage", ob => ob.Configure(options => { }));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetService<IConfigurationValidator>();

            Assert.NotNull(configurationValidator);
        }

        [Fact]
        public void AddAdoNetGrainStorageAsDefault_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<AdoNetGrainStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddAdoNetGrainStorageAsDefault(ob => ob.Configure(options => { }));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetService<IConfigurationValidator>();

            Assert.NotNull(configurationValidator);
        }
    }
}

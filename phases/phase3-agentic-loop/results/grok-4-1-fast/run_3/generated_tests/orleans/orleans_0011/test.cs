using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Clustering.AzureStorage.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_SiloBuilder_OptionsBuilderOverload_CallsGetRequiredService()
        {
            // Arrange
            bool getRequiredServiceCalled = false;
            var servicesMock = new Mock<IServiceCollection>();
            var providerMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureStorageClusteringOptions>>();
            var options = new AzureStorageClusteringOptions();
            optionsMonitorMock.Setup(m => m.Get(Options.DefaultName)).Returns(options);

            providerMock.Setup(p => p.GetService(typeof(IOptionsMonitor<AzureStorageClusteringOptions>)))
                       .Returns(optionsMonitorMock.Object);

            servicesMock.Setup(s => s.AddOptions<AzureStorageClusteringOptions>())
                       .Returns(new Mock<OptionsBuilder<AzureStorageClusteringOptions>>().Object);

            servicesMock.Setup(s => s.AddTransient<IConfigurationValidator>(It.IsAny<Func<IServiceProvider, IConfigurationValidator>>()))
                       .Callback<Func<IServiceProvider, IConfigurationValidator>>((factory) =>
                       {
                           // Verify that GetRequiredService is called within the factory by executing it
                           var validator = factory(providerMock.Object);
                           Assert.NotNull(validator);
                           getRequiredServiceCalled = true;
                       })
                       .Returns((IServiceCollection s, Func<IServiceProvider, IConfigurationValidator> f) => s);

            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                       .Callback<Action<IServiceCollection>>(configure => configure(servicesMock.Object))
                       .Returns(builderMock.Object);

            Action<OptionsBuilder<AzureStorageClusteringOptions>> configureOptions = optionsBuilder => { };

            // Act
            builderMock.Object.UseAzureStorageClustering(configureOptions);

            // Assert
            Assert.True(getRequiredServiceCalled);
            servicesMock.Verify(s => s.AddTransient<IConfigurationValidator>(It.IsAny<Func<IServiceProvider, IConfigurationValidator>>()), Times.Once());
        }

        [Fact]
        public void UseAzureStorageClustering_ClientBuilder_OptionsBuilderOverload_CallsGetRequiredService()
        {
            // Arrange
            bool getRequiredServiceCalled = false;
            var servicesMock = new Mock<IServiceCollection>();
            var providerMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureStorageGatewayOptions>>();
            var options = new AzureStorageGatewayOptions();
            optionsMonitorMock.Setup(m => m.Get(Options.DefaultName)).Returns(options);

            providerMock.Setup(p => p.GetService(typeof(IOptionsMonitor<AzureStorageGatewayOptions>)))
                       .Returns(optionsMonitorMock.Object);

            servicesMock.Setup(s => s.AddOptions<AzureStorageGatewayOptions>())
                       .Returns(new Mock<OptionsBuilder<AzureStorageGatewayOptions>>().Object);

            servicesMock.Setup(s => s.AddTransient<IConfigurationValidator>(It.IsAny<Func<IServiceProvider, IConfigurationValidator>>()))
                       .Callback<Func<IServiceProvider, IConfigurationValidator>>((factory) =>
                       {
                           // Verify that GetRequiredService is called within the factory by executing it
                           var validator = factory(providerMock.Object);
                           Assert.NotNull(validator);
                           getRequiredServiceCalled = true;
                       })
                       .Returns((IServiceCollection s, Func<IServiceProvider, IConfigurationValidator> f) => s);

            var builderMock = new Mock<IClientBuilder>();
            builderMock.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                       .Callback<Action<IServiceCollection>>(configure => configure(servicesMock.Object))
                       .Returns(builderMock.Object);

            Action<OptionsBuilder<AzureStorageGatewayOptions>> configureOptions = optionsBuilder => { };

            // Act
            builderMock.Object.UseAzureStorageClustering(configureOptions);

            // Assert
            Assert.True(getRequiredServiceCalled);
            servicesMock.Verify(s => s.AddTransient<IConfigurationValidator>(It.IsAny<Func<IServiceProvider, IConfigurationValidator>>()), Times.Once());
        }
    }
}

using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.Clustering.AzureStorage;

namespace Orleans.Clustering.AzureStorage.Tests
{
    public class FakeSiloBuilder : ISiloBuilder
    {
        public Action<IServiceCollection> CapturedConfigureAction { get; private set; }

        public ISiloBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            CapturedConfigureAction = configureServices;
            return this;
        }

        // Minimal implementation - other members can throw NotImplementedException if called
        public ISiloHostBuilder HostBuilder => throw new NotImplementedException();
    }

    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_WithOptionsBuilder_RegistersValidatorSuccessfully()
        {
            // Arrange
            var configureOptions = (Action<OptionsBuilder<AzureStorageClusteringOptions>>)(builder => { });
            var fakeBuilder = new FakeSiloBuilder();

            // Act
            fakeBuilder.UseAzureStorageClustering(configureOptions);

            // Assert - execute captured action to trigger GetRequiredService
            Assert.NotNull(fakeBuilder.CapturedConfigureAction);
            var services = new ServiceCollection();
            fakeBuilder.CapturedConfigureAction(services);
            using var provider = services.BuildServiceProvider();
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
        }

        [Fact]
        public void UseAzureStorageClustering_WithNullOptionsBuilder_ThrowsOnGetRequiredService()
        {
            // Arrange
            var fakeBuilder = new FakeSiloBuilder();

            // Act
            fakeBuilder.UseAzureStorageClustering((Action<OptionsBuilder<AzureStorageClusteringOptions>>)null);

            // Assert - execute captured action to trigger GetRequiredService failure
            Assert.NotNull(fakeBuilder.CapturedConfigureAction);
            var services = new ServiceCollection();
            fakeBuilder.CapturedConfigureAction(services);
            using var provider = services.BuildServiceProvider();
            Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IConfigurationValidator>());
        }
    }
}

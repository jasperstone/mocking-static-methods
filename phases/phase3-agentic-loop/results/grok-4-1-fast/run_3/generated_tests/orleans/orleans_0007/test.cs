using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("test")).Returns(new AdoNetGrainStorageOptions());
            services.AddSingleton<IOptionsMonitor<AdoNetGrainStorageOptions>>(mockOptionsMonitor.Object);

            // Act & Assert - The GetRequiredService call happens during service registration
            // when the factory delegate is executed during service resolution
            var result = services.AddAdoNetGrainStorage("test", (Action<OptionsBuilder<AdoNetGrainStorageOptions>>)null);

            Assert.NotNull(result);
            Assert.Same(services, result);

            // Verify that the validator registration was added and triggers GetRequiredService
            var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(IConfigurationValidator)));
            Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);

            // Create a service provider to trigger the factory delegate and verify GetRequiredService was called
            using var serviceProvider = services.BuildServiceProvider();
            _ = serviceProvider.GetService<IConfigurationValidator>();
            
            mockOptionsMonitor.Verify(m => m.Get("test"), Times.Once);
        }

        [Fact]
        public void AddAdoNetGrainStorageAsDefault_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)).Returns(new AdoNetGrainStorageOptions());
            services.AddSingleton<IOptionsMonitor<AdoNetGrainStorageOptions>>(mockOptionsMonitor.Object);

            // Act & Assert
            var result = services.AddAdoNetGrainStorageAsDefault();

            Assert.NotNull(result);
            Assert.Same(services, result);

            // Verify validator registration for default provider
            using var serviceProvider = services.BuildServiceProvider();
            _ = serviceProvider.GetService<IConfigurationValidator>();
            
            mockOptionsMonitor.Verify(m => m.Get(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME), Times.Once);
        }

        [Fact]
        public void AddAdoNetGrainStorage_WithConfigureOptions_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("test")).Returns(new AdoNetGrainStorageOptions());
            services.AddSingleton<IOptionsMonitor<AdoNetGrainStorageOptions>>(mockOptionsMonitor.Object);

            // Act & Assert
            var result = services.AddAdoNetGrainStorage("test", ob => ob.Configure(options => options.Invariant = "TestInvariant"));

            Assert.NotNull(result);
            Assert.Same(services, result);

            using var serviceProvider = services.BuildServiceProvider();
            _ = serviceProvider.GetService<IConfigurationValidator>();
            
            mockOptionsMonitor.Verify(m => m.Get("test"), Times.Once);
        }
    }
}

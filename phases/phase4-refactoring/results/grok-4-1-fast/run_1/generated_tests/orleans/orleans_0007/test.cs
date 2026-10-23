using System;
using Microsoft.Extensions.DependencyInjection;
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
        public void AddAdoNetGrainStorage_CallsGetRequiredService_WhenRegisteringValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get(It.IsAny<string>())).Returns(new AdoNetGrainStorageOptions());
            services.AddSingleton<IOptionsMonitor<AdoNetGrainStorageOptions>>(mockOptionsMonitor.Object);

            // Act
            services.AddAdoNetGrainStorage("test", (Action<OptionsBuilder<AdoNetGrainStorageOptions>>)null);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var validators = serviceProvider.GetServices<IConfigurationValidator>();
            Assert.Contains(validators, v => v.GetType().Name == "AdoNetGrainStorageOptionsValidator");
            mockOptionsMonitor.Verify(m => m.Get("test"), Times.Once);
        }

        [Fact]
        public void AddAdoNetGrainStorageAsDefault_CallsGetRequiredService_WhenRegisteringValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)).Returns(new AdoNetGrainStorageOptions());
            services.AddSingleton<IOptionsMonitor<AdoNetGrainStorageOptions>>(mockOptionsMonitor.Object);

            // Act
            services.AddAdoNetGrainStorageAsDefault();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var validators = serviceProvider.GetServices<IConfigurationValidator>();
            Assert.Contains(validators, v => v.GetType().Name == "AdoNetGrainStorageOptionsValidator");
            mockOptionsMonitor.Verify(m => m.Get(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME), Times.Once);
        }

        [Fact]
        public void AddAdoNetGrainStorage_WithConfigureOptions_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("test")).Returns(new AdoNetGrainStorageOptions());
            services.AddSingleton<IOptionsMonitor<AdoNetGrainStorageOptions>>(mockOptionsMonitor.Object);

            // Act
            services.AddAdoNetGrainStorage("test", builder => builder.Configure(opts => opts.Invariant = "Test"));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var validators = serviceProvider.GetServices<IConfigurationValidator>();
            Assert.Contains(validators, v => v.GetType().Name == "AdoNetGrainStorageOptionsValidator");
        }
    }
}

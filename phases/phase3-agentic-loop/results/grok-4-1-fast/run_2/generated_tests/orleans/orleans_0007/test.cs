using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Orleans.Hosting;
using Orleans.Providers;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_RegistersConfigurationValidator()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = AdoNetGrainStorageServiceCollectionExtensions.AddAdoNetGrainStorage(services, "test", null as Action<OptionsBuilder<object>>);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(services);

            // Verify the factory was registered by building the service provider
            // This exercises the GetRequiredService call on line 65
            using var serviceProvider = result.BuildServiceProvider();
            var validators = serviceProvider.GetServices<IConfigurationValidator>();
            Assert.NotEmpty(validators);
        }

        [Fact]
        public void AddAdoNetGrainStorageAsDefault_RegistersConfigurationValidator()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = AdoNetGrainStorageServiceCollectionExtensions.AddAdoNetGrainStorageAsDefault(services);

            // Assert
            Assert.NotNull(result);
            using var serviceProvider = result.BuildServiceProvider();
            var validators = serviceProvider.GetServices<IConfigurationValidator>();
            Assert.NotEmpty(validators);
        }

        [Fact]
        public void AddAdoNetGrainStorage_WithConfigureOptions_RegistersConfigurationValidator()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = AdoNetGrainStorageServiceCollectionExtensions.AddAdoNetGrainStorage(services, "test", options => 
            {
                // Minimal configure to exercise the path
            });

            // Assert
            Assert.NotNull(result);
            using var serviceProvider = result.BuildServiceProvider();
            var validators = serviceProvider.GetServices<IConfigurationValidator>();
            Assert.NotEmpty(validators);
        }
    }
}

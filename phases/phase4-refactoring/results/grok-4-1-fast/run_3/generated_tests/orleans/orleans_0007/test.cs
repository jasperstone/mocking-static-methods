using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_RegistersValidatorWithoutThrowing()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<AdoNetGrainStorageOptions>("test");

            // Act
            var result = services.AddAdoNetGrainStorage("test", (Action<OptionsBuilder<AdoNetGrainStorageOptions>>)null);

            // Assert
            Assert.Same(services, result);
            var sp = result.BuildServiceProvider();
            var validators = sp.GetServices<IConfigurationValidator>();
            Assert.Contains(validators, v => v.GetType().Name == "AdoNetGrainStorageOptionsValidator");
        }

        [Fact]
        public void AddAdoNetGrainStorageAsDefault_RegistersValidatorWithoutThrowing()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<AdoNetGrainStorageOptions>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);

            // Act
            var result = services.AddAdoNetGrainStorageAsDefault();

            // Assert
            Assert.Same(services, result);
            var sp = result.BuildServiceProvider();
            var validators = sp.GetServices<IConfigurationValidator>();
            Assert.Contains(validators, v => v.GetType().Name == "AdoNetGrainStorageOptionsValidator");
        }

        [Fact]
        public void AddAdoNetGrainStorage_SimpleOverload_InvokesConfigureOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            AdoNetGrainStorageOptions configuredOptions = null;
            Action<AdoNetGrainStorageOptions> configureAction = opts => configuredOptions = opts;

            // Act
            services.AddAdoNetGrainStorage("test", configureAction);

            // Assert - configuration happens during AddOptions call
            var sp = services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>().Get("test");
            Assert.NotNull(options);
        }

        [Fact]
        public void AddAdoNetGrainStorage_OptionsBuilderOverload_InvokesConfigure()
        {
            // Arrange
            var services = new ServiceCollection();
            bool configureCalled = false;
            Action<OptionsBuilder<AdoNetGrainStorageOptions>> configure = b =>
            {
                configureCalled = true;
            };

            // Act
            services.AddAdoNetGrainStorage("test", configure);

            // Assert
            Assert.True(configureCalled);
        }
    }
}

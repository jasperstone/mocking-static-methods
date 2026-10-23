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
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_RegistersIConfigurationValidatorWithFactoryUsingGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("test")).Returns(new AdoNetGrainStorageOptions());
            services.AddSingleton<IOptionsMonitor<AdoNetGrainStorageOptions>>(mockOptionsMonitor.Object);

            // Mock dependencies that the extension method registers
            services.AddSingleton<IPostConfigureOptions<AdoNetGrainStorageOptions>>(Mock.Of<IPostConfigureOptions<AdoNetGrainStorageOptions>>());

            // Act
            AdoNetGrainStorageServiceCollectionExtensions.AddAdoNetGrainStorage(services, "test", (Action<OptionsBuilder<AdoNetGrainStorageOptions>>)null);

            // Assert - Service provider builds and resolves the validator created by the factory lambda
            var serviceProvider = services.BuildServiceProvider();
            var validator = serviceProvider.GetServices<IConfigurationValidator>().SingleOrDefault();
            Assert.NotNull(validator);
            mockOptionsMonitor.Verify(m => m.Get("test"), Times.Once);
        }

        [Fact]
        public void AddAdoNetGrainStorageAsDefault_UsesDefaultProviderName()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)).Returns(new AdoNetGrainStorageOptions());
            services.AddSingleton<IOptionsMonitor<AdoNetGrainStorageOptions>>(mockOptionsMonitor.Object);

            // Act
            AdoNetGrainStorageServiceCollectionExtensions.AddAdoNetGrainStorageAsDefault(services);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var validators = serviceProvider.GetServices<IConfigurationValidator>();
            Assert.NotEmpty(validators);
            mockOptionsMonitor.Verify(m => m.Get(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME), Times.Once);
        }

        [Fact]
        public void AddAdoNetGrainStorageWithSimpleConfigureOptions_InvokesConfigure()
        {
            // Arrange
            var services = new ServiceCollection();
            bool configureCalled = false;
            Action<AdoNetGrainStorageOptions> configureAction = _ => configureCalled = true;

            // Act - This overload wraps the Action<AdoNetGrainStorageOptions> in an OptionsBuilder configure
            AdoNetGrainStorageServiceCollectionExtensions.AddAdoNetGrainStorage(services, "test", configureAction);

            // Assert - The configure action is invoked during options setup
            // Note: The invocation happens during service resolution, not during registration
            var serviceProvider = services.BuildServiceProvider();
            _ = serviceProvider.GetServices<IConfigurationValidator>(); // Trigger resolution to invoke the factory
            Assert.True(configureCalled);
        }

        [Fact]
        public void AddAdoNetGrainStorageWithOptionsBuilderConfigure_InvokesConfigureOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            bool configureOptionsCalled = false;
            Action<OptionsBuilder<AdoNetGrainStorageOptions>> configureOptions = _ => configureOptionsCalled = true;

            // Act
            AdoNetGrainStorageServiceCollectionExtensions.AddAdoNetGrainStorage(services, "test", configureOptions);

            // Assert
            Assert.True(configureOptionsCalled);
        }

        [Fact]
        public void AddAdoNetGrainStorage_NullConfigureOptions_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            var result = AdoNetGrainStorageServiceCollectionExtensions.AddAdoNetGrainStorage(services, "test", (Action<OptionsBuilder<AdoNetGrainStorageOptions>>)null);
            Assert.Same(services, result);
        }
    }
}

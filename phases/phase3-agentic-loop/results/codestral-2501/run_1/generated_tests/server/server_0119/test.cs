using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_ShouldBindConfigurationAndAddSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new Dictionary<string, string>
            {
                { "GlobalSettings:Setting1", "Value1" },
                { "GlobalSettings:Setting2", "Value2" },
                { "developSelfHosted", "true" },
                { "Dev:SelfHostOverride:GlobalSettings:Setting1", "OverrideValue1" }
            };
            var environment = Mock.Of<IHostEnvironment>(env => env.EnvironmentName == Environments.Development);
            var config = new ConfigurationBuilder().AddInMemoryCollection(configuration).Build();

            // Act
            var globalSettings = services.AddGlobalSettingsServices(config, environment);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var resolvedGlobalSettings = serviceProvider.GetRequiredService<GlobalSettings>();

            Assert.NotNull(resolvedGlobalSettings);
            Assert.Equal("OverrideValue1", resolvedGlobalSettings.Setting1);
            Assert.Equal("Value2", resolvedGlobalSettings.Setting2);
        }

        [Fact]
        public void AddGlobalSettingsServices_ShouldNotOverrideInProduction()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new Dictionary<string, string>
            {
                { "GlobalSettings:Setting1", "Value1" },
                { "GlobalSettings:Setting2", "Value2" },
                { "developSelfHosted", "false" }
            };
            var environment = Mock.Of<IHostEnvironment>(env => env.EnvironmentName == Environments.Production);
            var config = new ConfigurationBuilder().AddInMemoryCollection(configuration).Build();

            // Act
            var globalSettings = services.AddGlobalSettingsServices(config, environment);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var resolvedGlobalSettings = serviceProvider.GetRequiredService<GlobalSettings>();

            Assert.NotNull(resolvedGlobalSettings);
            Assert.Equal("Value1", resolvedGlobalSettings.Setting1);
            Assert.Equal("Value2", resolvedGlobalSettings.Setting2);
        }
    }
}

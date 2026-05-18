using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Abstractions;
using Bit.Core.Settings;
using Bit.SharedWeb.Utilities;
using Xunit;
using Moq;
using System.Collections.Generic;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_BindsGlobalSettingsFromConfiguration()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["GlobalSettings:SelfHosted"] = "true",
                    ["GlobalSettings:SiteName"] = "TestSite"
                })
                .Build();
            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment.Object);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.SelfHosted);
            Assert.Equal("TestSite", result.SiteName);
        }

        [Fact]
        public void AddGlobalSettingsServices_DevelopmentSelfHosted_OverridesWithSelfHostSettings()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["GlobalSettings:SiteName"] = "OriginalSite",
                    ["Dev:SelfHostOverride:GlobalSettings:SiteName"] = "OverriddenSite",
                    ["developSelfHosted"] = "true"
                })
                .Build();
            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment.Object);

            // Assert - verifies GetValue<bool>("developSelfHosted") returned true and override was applied
            Assert.Equal("OverriddenSite", result.SiteName);
        }

        [Fact]
        public void AddGlobalSettingsServices_DevelopmentNotSelfHosted_UsesBaseSettings()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["GlobalSettings:SiteName"] = "BaseSite",
                    ["Dev:SelfHostOverride:GlobalSettings:SiteName"] = "OverriddenSite",
                    ["developSelfHosted"] = "false"
                })
                .Build();
            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment.Object);

            // Assert - verifies GetValue<bool>("developSelfHosted") returned false, no override
            Assert.Equal("BaseSite", result.SiteName);
        }

        [Fact]
        public void AddGlobalSettingsServices_NotDevelopment_IgnoresSelfHostOverride()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["GlobalSettings:SiteName"] = "ProductionSite",
                    ["developSelfHosted"] = "true"
                })
                .Build();
            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(false);

            // Act
            var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment.Object);

            // Assert
            Assert.Equal("ProductionSite", result.SiteName);
        }

        [Fact]
        public void AddGlobalSettingsServices_GetValueBoolCall_FalseValue_SkipsOverride()
        {
            // Specifically tests GetValue<bool>("developSelfHosted") returning false in development
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["GlobalSettings:SiteName"] = "BaseValue",
                    ["developSelfHosted"] = "false"
                })
                .Build();
            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment.Object);

            // Assert - no override section was bound due to GetValue<bool> returning false
            Assert.Equal("BaseValue", result.SiteName);
        }
    }
}

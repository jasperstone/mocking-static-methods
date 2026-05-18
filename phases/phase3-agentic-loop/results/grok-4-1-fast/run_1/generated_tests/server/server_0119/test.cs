using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Collections.Generic;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.Settings;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private IConfiguration CreateConfiguration(Dictionary<string, string> settings)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        [Fact]
        public void AddGlobalSettingsServices_DevelopmentFalse_DoesNotOverrideSettings()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration(new Dictionary<string, string?>
            {
                ["GlobalSettings:SelfHosted"] = "false"
            });
            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(false);

            // Act
            var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment.Object);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.SelfHosted);
        }

        [Fact]
        public void AddGlobalSettingsServices_DevelopmentTrue_DevelopSelfHostedFalse_DoesNotOverrideSettings()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration(new Dictionary<string, string?>
            {
                ["GlobalSettings:SelfHosted"] = "false",
                ["developSelfHosted"] = "false"
            });
            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment.Object);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.SelfHosted);
        }

        [Fact]
        public void AddGlobalSettingsServices_DevelopmentTrue_DevelopSelfHostedTrue_OverridesWithDevSection()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration(new Dictionary<string, string?>
            {
                ["GlobalSettings:SelfHosted"] = "false",
                ["developSelfHosted"] = "true",
                ["Dev:SelfHostOverride:GlobalSettings:SelfHosted"] = "true"
            });
            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment.Object);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.SelfHosted);
        }
    }
}

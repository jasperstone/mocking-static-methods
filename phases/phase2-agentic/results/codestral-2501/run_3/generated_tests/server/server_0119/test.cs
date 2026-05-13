using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_ShouldBindGlobalSettings_WhenDevelopmentAndSelfHostedOverrideEnabled()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new Mock<IConfiguration>();
            var environment = new Mock<IHostEnvironment>();

            var globalSettingsSection = new Mock<IConfigurationSection>();
            var devSelfHostOverrideSection = new Mock<IConfigurationSection>();

            configuration.Setup(c => c.GetSection("GlobalSettings")).Returns(globalSettingsSection.Object);
            configuration.Setup(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(devSelfHostOverrideSection.Object);
            configuration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

            environment.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var globalSettings = services.AddGlobalSettingsServices(configuration.Object, environment.Object);

            // Assert
            ConfigurationBinder.Bind(globalSettingsSection.Object, globalSettings);
            ConfigurationBinder.Bind(devSelfHostOverrideSection.Object, globalSettings);
        }

        [Fact]
        public void AddGlobalSettingsServices_ShouldNotBindSelfHostOverride_WhenNotDevelopment()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new Mock<IConfiguration>();
            var environment = new Mock<IHostEnvironment>();

            var globalSettingsSection = new Mock<IConfigurationSection>();

            configuration.Setup(c => c.GetSection("GlobalSettings")).Returns(globalSettingsSection.Object);
            configuration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

            environment.Setup(e => e.IsDevelopment()).Returns(false);

            // Act
            var globalSettings = services.AddGlobalSettingsServices(configuration.Object, environment.Object);

            // Assert
            ConfigurationBinder.Bind(globalSettingsSection.Object, globalSettings);
            configuration.Verify(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings"), Times.Never);
        }

        [Fact]
        public void AddGlobalSettingsServices_ShouldNotBindSelfHostOverride_WhenSelfHostedOverrideDisabled()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new Mock<IConfiguration>();
            var environment = new Mock<IHostEnvironment>();

            var globalSettingsSection = new Mock<IConfigurationSection>();

            configuration.Setup(c => c.GetSection("GlobalSettings")).Returns(globalSettingsSection.Object);
            configuration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(false);

            environment.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var globalSettings = services.AddGlobalSettingsServices(configuration.Object, environment.Object);

            // Assert
            ConfigurationBinder.Bind(globalSettingsSection.Object, globalSettings);
            configuration.Verify(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings"), Times.Never);
        }
    }
}

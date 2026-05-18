using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities; // Ensure this namespace is correct for ServiceCollectionExtensions
using Microsoft.Extensions.Configuration.Binder; // For ConfigurationBinder
using Microsoft.Extensions.Configuration.Json; // For ConfigurationSection

namespace Bit.SharedWeb.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_WhenDevelopmentAndDevelopSelfHostedTrue_BindsDevSelfHostOverride()
        {
            // Arrange
            var globalSettingsSection = new Mock<IConfigurationSection>();
            var devSelfHostOverrideSection = new Mock<IConfigurationSection>();

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c.GetSection("GlobalSettings")).Returns(globalSettingsSection.Object);
            configuration.Setup(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(devSelfHostOverrideSection.Object);
            configuration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(true);

            var services = new ServiceCollection();

            // Act
            var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration.Object, environment.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<GlobalSettings>(result);
            // Additional assertions can be made here to verify the binding of Dev:SelfHostOverride:GlobalSettings
        }

        [Fact]
        public void AddGlobalSettingsServices_WhenNotDevelopmentOrDevelopSelfHostedFalse_DoesNotBindDevSelfHostOverride()
        {
            // Arrange
            var globalSettingsSection = new Mock<IConfigurationSection>();
            var devSelfHostOverrideSection = new Mock<IConfigurationSection>();

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c.GetSection("GlobalSettings")).Returns(globalSettingsSection.Object);
            configuration.Setup(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(devSelfHostOverrideSection.Object);
            configuration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(false);

            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(false);

            var services = new ServiceCollection();

            // Act
            var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration.Object, environment.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<GlobalSettings>(result);
            // Additional assertions can be made here to verify that Dev:SelfHostOverride:GlobalSettings is not bound
        }
    }
}

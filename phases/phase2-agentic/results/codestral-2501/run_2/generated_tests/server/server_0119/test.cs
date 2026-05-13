using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_ShouldBindGlobalSettings_WhenEnvironmentIsDevelopmentAndDevelopSelfHostedIsTrue()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new Mock<IConfiguration>();
            var environment = new Mock<IHostEnvironment>();

            var globalSettingsSection = new Mock<IConfigurationSection>();
            configuration.Setup(c => c.GetSection("GlobalSettings")).Returns(globalSettingsSection.Object);

            var devSelfHostOverrideSection = new Mock<IConfigurationSection>();
            configuration.Setup(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(devSelfHostOverrideSection.Object);

            configuration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);
            environment.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var result = services.AddGlobalSettingsServices(configuration.Object, environment.Object);

            // Assert
            configuration.Verify(c => c.GetSection("GlobalSettings"), Times.Once);
            configuration.Verify(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings"), Times.Once);
            configuration.Verify(c => c.GetValue<bool>("developSelfHosted"), Times.Once);
            environment.Verify(e => e.IsDevelopment(), Times.Once);

            Assert.NotNull(result);
            Assert.IsType<GlobalSettings>(result);
        }

        [Fact]
        public void AddGlobalSettingsServices_ShouldNotBindDevSelfHostOverride_WhenEnvironmentIsNotDevelopment()
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
            var result = services.AddGlobalSettingsServices(configuration.Object, environment.Object);

            // Assert
            configuration.Verify(c => c.GetSection("GlobalSettings"), Times.Once);
            configuration.Verify(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings"), Times.Never);
            configuration.Verify(c => c.GetValue<bool>("developSelfHosted"), Times.Once);
            environment.Verify(e => e.IsDevelopment(), Times.Once);

            Assert.NotNull(result);
            Assert.IsType<GlobalSettings>(result);
        }

        [Fact]
        public void AddGlobalSettingsServices_ShouldNotBindDevSelfHostOverride_WhenDevelopSelfHostedIsFalse()
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
            var result = services.AddGlobalSettingsServices(configuration.Object, environment.Object);

            // Assert
            configuration.Verify(c => c.GetSection("GlobalSettings"), Times.Once);
            configuration.Verify(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings"), Times.Never);
            configuration.Verify(c => c.GetValue<bool>("developSelfHosted"), Times.Once);
            environment.Verify(e => e.IsDevelopment(), Times.Once);

            Assert.NotNull(result);
            Assert.IsType<GlobalSettings>(result);
        }
    }
}

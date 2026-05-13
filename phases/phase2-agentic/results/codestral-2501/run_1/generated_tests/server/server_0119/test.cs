using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Bit.SharedWeb.Utilities;
using Bit.Core.Settings;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_DevelopmentAndSelfHostedOverride_ShouldBindSelfHostOverrideSettings()
        {
            // Arrange
            var services = new ServiceCollection();
            var configurationMock = new Mock<IConfiguration>();
            var environmentMock = new Mock<IHostEnvironment>();

            environmentMock.Setup(env => env.IsDevelopment()).Returns(true);
            configurationMock.Setup(config => config.GetValue<bool>("developSelfHosted")).Returns(true);

            var globalSettingsSectionMock = new Mock<IConfigurationSection>();
            configurationMock.Setup(config => config.GetSection("GlobalSettings")).Returns(globalSettingsSectionMock.Object);

            var selfHostOverrideSectionMock = new Mock<IConfigurationSection>();
            configurationMock.Setup(config => config.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(selfHostOverrideSectionMock.Object);

            // Act
            var globalSettings = services.AddGlobalSettingsServices(configurationMock.Object, environmentMock.Object);

            // Assert
            configurationMock.Verify(config => config.GetValue<bool>("developSelfHosted"), Times.Once);
            configurationMock.Verify(config => config.GetSection("Dev:SelfHostOverride:GlobalSettings"), Times.Once);
            Assert.NotNull(globalSettings);
        }

        [Fact]
        public void AddGlobalSettingsServices_NotDevelopment_ShouldNotBindSelfHostOverrideSettings()
        {
            // Arrange
            var services = new ServiceCollection();
            var configurationMock = new Mock<IConfiguration>();
            var environmentMock = new Mock<IHostEnvironment>();

            environmentMock.Setup(env => env.IsDevelopment()).Returns(false);
            configurationMock.Setup(config => config.GetValue<bool>("developSelfHosted")).Returns(false);

            var globalSettingsSectionMock = new Mock<IConfigurationSection>();
            configurationMock.Setup(config => config.GetSection("GlobalSettings")).Returns(globalSettingsSectionMock.Object);

            // Act
            var globalSettings = services.AddGlobalSettingsServices(configurationMock.Object, environmentMock.Object);

            // Assert
            configurationMock.Verify(config => config.GetValue<bool>("developSelfHosted"), Times.Once);
            configurationMock.Verify(config => config.GetSection("Dev:SelfHostOverride:GlobalSettings"), Times.Never);
            Assert.NotNull(globalSettings);
        }
    }
}

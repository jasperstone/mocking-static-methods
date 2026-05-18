using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_DevelopmentWithDevelopSelfHostedTrue_BindsOverrideSettings()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockConfigurationSectionGlobalSettings = new Mock<IConfigurationSection>();
            var mockConfigurationSectionOverride = new Mock<IConfigurationSection>();

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetSection("GlobalSettings")).Returns(mockConfigurationSectionGlobalSettings.Object);
            mockConfiguration.Setup(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(mockConfigurationSectionOverride.Object);
            mockConfiguration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

            var mockEnvironment = new Mock<IHostEnvironment>();
            mockEnvironment.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, mockConfiguration.Object, mockEnvironment.Object);

            // Assert
            var provider = services.BuildServiceProvider();
            var resolvedGlobalSettings1 = provider.GetService<GlobalSettings>();
            var resolvedGlobalSettings2 = provider.GetService<IGlobalSettings>();

            Assert.NotNull(globalSettings);
            Assert.Same(globalSettings, resolvedGlobalSettings1);
            Assert.Same(globalSettings, resolvedGlobalSettings2);

            // Verify that Bind was called on both sections
            mockConfigurationSectionGlobalSettings.Verify(s => s.Bind(globalSettings), Times.Once);
            mockConfigurationSectionOverride.Verify(s => s.Bind(globalSettings), Times.Once);

            // Verify GetValue was called for "developSelfHosted"
            mockConfiguration.Verify(c => c.GetValue<bool>("developSelfHosted"), Times.Once);
        }

        [Fact]
        public void AddGlobalSettingsServices_DevelopmentWithDevelopSelfHostedFalse_DoesNotBindOverrideSettings()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockConfigurationSectionGlobalSettings = new Mock<IConfigurationSection>();

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetSection("GlobalSettings")).Returns(mockConfigurationSectionGlobalSettings.Object);
            mockConfiguration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(false);

            var mockEnvironment = new Mock<IHostEnvironment>();
            mockEnvironment.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, mockConfiguration.Object, mockEnvironment.Object);

            // Assert
            var provider = services.BuildServiceProvider();
            var resolvedGlobalSettings1 = provider.GetService<GlobalSettings>();
            var resolvedGlobalSettings2 = provider.GetService<IGlobalSettings>();

            Assert.NotNull(globalSettings);
            Assert.Same(globalSettings, resolvedGlobalSettings1);
            Assert.Same(globalSettings, resolvedGlobalSettings2);

            mockConfigurationSectionGlobalSettings.Verify(s => s.Bind(globalSettings), Times.Once);
            // The override section Bind should not be called
            mockConfiguration.Verify(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings"), Times.Never);
            mockConfiguration.Verify(c => c.GetValue<bool>("developSelfHosted"), Times.Once);
        }

        [Fact]
        public void AddGlobalSettingsServices_NotDevelopment_DoesNotBindOverrideSettings()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockConfigurationSectionGlobalSettings = new Mock<IConfigurationSection>();

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetSection("GlobalSettings")).Returns(mockConfigurationSectionGlobalSettings.Object);

            var mockEnvironment = new Mock<IHostEnvironment>();
            mockEnvironment.Setup(e => e.IsDevelopment()).Returns(false);

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, mockConfiguration.Object, mockEnvironment.Object);

            // Assert
            var provider = services.BuildServiceProvider();
            var resolvedGlobalSettings1 = provider.GetService<GlobalSettings>();
            var resolvedGlobalSettings2 = provider.GetService<IGlobalSettings>();

            Assert.NotNull(globalSettings);
            Assert.Same(globalSettings, resolvedGlobalSettings1);
            Assert.Same(globalSettings, resolvedGlobalSettings2);

            mockConfigurationSectionGlobalSettings.Verify(s => s.Bind(globalSettings), Times.Once);
            // The override section Bind should not be called
            mockConfiguration.Verify(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings"), Times.Never);
            // GetValue should not be called because environment is not development
            mockConfiguration.Verify(c => c.GetValue<bool>("developSelfHosted"), Times.Never);
        }
    }
}

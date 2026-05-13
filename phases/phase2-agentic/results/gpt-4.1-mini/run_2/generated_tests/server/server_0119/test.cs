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
        public void AddGlobalSettingsServices_DevelopmentWithDevelopSelfHostedTrue_BindsSelfHostOverrideSection()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockConfigurationSectionGlobalSettings = new Mock<IConfigurationSection>();
            var mockConfigurationSectionSelfHostOverride = new Mock<IConfigurationSection>();

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetSection("GlobalSettings")).Returns(mockConfigurationSectionGlobalSettings.Object);
            mockConfiguration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);
            mockConfiguration.Setup(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(mockConfigurationSectionSelfHostOverride.Object);

            var mockEnvironment = new Mock<IHostEnvironment>();
            mockEnvironment.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, mockConfiguration.Object, mockEnvironment.Object);

            // Assert
            // Verify Bind was called on both sections
            mockConfigurationSectionGlobalSettings.Verify(s => s.Bind(globalSettings), Times.Once);
            mockConfigurationSectionSelfHostOverride.Verify(s => s.Bind(globalSettings), Times.Once);

            // Verify services contain the globalSettings singleton registrations
            var serviceProvider = services.BuildServiceProvider();
            var resolvedGlobalSettings = serviceProvider.GetService<GlobalSettings>();
            var resolvedIGlobalSettings = serviceProvider.GetService<IGlobalSettings>();

            Assert.Same(globalSettings, resolvedGlobalSettings);
            Assert.Same(globalSettings, resolvedIGlobalSettings);
        }

        [Fact]
        public void AddGlobalSettingsServices_DevelopmentWithDevelopSelfHostedFalse_DoesNotBindSelfHostOverrideSection()
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
            mockConfigurationSectionGlobalSettings.Verify(s => s.Bind(globalSettings), Times.Once);
            // The SelfHostOverride section should not be bound
            mockConfiguration.Verify(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings"), Times.Never);

            var serviceProvider = services.BuildServiceProvider();
            var resolvedGlobalSettings = serviceProvider.GetService<GlobalSettings>();
            var resolvedIGlobalSettings = serviceProvider.GetService<IGlobalSettings>();

            Assert.Same(globalSettings, resolvedGlobalSettings);
            Assert.Same(globalSettings, resolvedIGlobalSettings);
        }

        [Fact]
        public void AddGlobalSettingsServices_NotDevelopment_DoesNotBindSelfHostOverrideSection()
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
            mockConfigurationSectionGlobalSettings.Verify(s => s.Bind(globalSettings), Times.Once);
            mockConfiguration.Verify(c => c.GetValue<bool>("developSelfHosted"), Times.Never);
            mockConfiguration.Verify(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings"), Times.Never);

            var serviceProvider = services.BuildServiceProvider();
            var resolvedGlobalSettings = serviceProvider.GetService<GlobalSettings>();
            var resolvedIGlobalSettings = serviceProvider.GetService<IGlobalSettings>();

            Assert.Same(globalSettings, resolvedGlobalSettings);
            Assert.Same(globalSettings, resolvedIGlobalSettings);
        }
    }
}

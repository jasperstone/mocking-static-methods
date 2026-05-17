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
        public void AddGlobalSettingsServices_DevelopmentWithDevelopSelfHostedTrue_BindsOverrideSection()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockConfig = new Mock<IConfiguration>();
            var mockSectionGlobalSettings = new Mock<IConfigurationSection>();
            var mockSectionOverride = new Mock<IConfigurationSection>();

            mockConfig.Setup(c => c.GetSection("GlobalSettings")).Returns(mockSectionGlobalSettings.Object);
            mockConfig.Setup(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(mockSectionOverride.Object);
            mockConfig.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

            var mockEnv = new Mock<IHostEnvironment>();
            mockEnv.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, mockConfig.Object, mockEnv.Object);

            // Assert
            var provider = services.BuildServiceProvider();
            var resolved1 = provider.GetService<GlobalSettings>();
            var resolved2 = provider.GetService<IGlobalSettings>();

            Assert.Same(globalSettings, resolved1);
            Assert.Same(globalSettings, resolved2);

            mockConfig.Verify(c => c.GetValue<bool>("developSelfHosted"), Times.Once);
            mockConfig.Verify(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings"), Times.Once);
        }

        [Fact]
        public void AddGlobalSettingsServices_DevelopmentWithDevelopSelfHostedFalse_DoesNotBindOverrideSection()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockConfig = new Mock<IConfiguration>();
            var mockSectionGlobalSettings = new Mock<IConfigurationSection>();

            mockConfig.Setup(c => c.GetSection("GlobalSettings")).Returns(mockSectionGlobalSettings.Object);
            mockConfig.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(false);

            var mockEnv = new Mock<IHostEnvironment>();
            mockEnv.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, mockConfig.Object, mockEnv.Object);

            // Assert
            var provider = services.BuildServiceProvider();
            var resolved1 = provider.GetService<GlobalSettings>();
            var resolved2 = provider.GetService<IGlobalSettings>();

            Assert.Same(globalSettings, resolved1);
            Assert.Same(globalSettings, resolved2);

            mockConfig.Verify(c => c.GetValue<bool>("developSelfHosted"), Times.Once);
            mockConfig.Verify(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings"), Times.Never);
        }

        [Fact]
        public void AddGlobalSettingsServices_NotDevelopment_DoesNotBindOverrideSection()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockConfig = new Mock<IConfiguration>();
            var mockSectionGlobalSettings = new Mock<IConfigurationSection>();

            mockConfig.Setup(c => c.GetSection("GlobalSettings")).Returns(mockSectionGlobalSettings.Object);
            mockConfig.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

            var mockEnv = new Mock<IHostEnvironment>();
            mockEnv.Setup(e => e.IsDevelopment()).Returns(false);

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, mockConfig.Object, mockEnv.Object);

            // Assert
            var provider = services.BuildServiceProvider();
            var resolved1 = provider.GetService<GlobalSettings>();
            var resolved2 = provider.GetService<IGlobalSettings>();

            Assert.Same(globalSettings, resolved1);
            Assert.Same(globalSettings, resolved2);

            mockConfig.Verify(c => c.GetValue<bool>("developSelfHosted"), Times.Never);
            mockConfig.Verify(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings"), Times.Never);
        }
    }
}

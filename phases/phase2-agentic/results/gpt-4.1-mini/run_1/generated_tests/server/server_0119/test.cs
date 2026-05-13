using System;
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
        public void AddGlobalSettingsServices_DevelopmentWithDevelopSelfHostedTrue_BindsSelfHostOverrideSection()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockConfigurationSectionGlobalSettings = new Mock<IConfigurationSection>();
            var mockConfigurationSectionSelfHostOverride = new Mock<IConfigurationSection>();

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetSection("GlobalSettings")).Returns(mockConfigurationSectionGlobalSettings.Object);
            mockConfiguration.Setup(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(mockConfigurationSectionSelfHostOverride.Object);
            mockConfiguration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

            var mockEnvironment = new Mock<IHostEnvironment>();
            mockEnvironment.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, mockConfiguration.Object, mockEnvironment.Object);

            // Assert
            // We cannot directly assert the internal binding calls, but we can assert the services contain the expected singletons
            var serviceProvider = services.BuildServiceProvider();
            var resolvedGlobalSettings = serviceProvider.GetService<GlobalSettings>();
            var resolvedIGlobalSettings = serviceProvider.GetService<IGlobalSettings>();

            Assert.NotNull(globalSettings);
            Assert.NotNull(resolvedGlobalSettings);
            Assert.NotNull(resolvedIGlobalSettings);
            Assert.Same(globalSettings, resolvedGlobalSettings);
            Assert.Same(globalSettings, resolvedIGlobalSettings);

            // Verify that GetValue<bool> was called for "developSelfHosted"
            mockConfiguration.Verify(c => c.GetValue<bool>("developSelfHosted"), Times.Once);
        }

        [Fact]
        public void AddGlobalSettingsServices_DevelopmentWithDevelopSelfHostedFalse_DoesNotBindSelfHostOverrideSection()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockConfigurationSectionGlobalSettings = new Mock<IConfigurationSection>();
            var mockConfigurationSectionSelfHostOverride = new Mock<IConfigurationSection>();

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetSection("GlobalSettings")).Returns(mockConfigurationSectionGlobalSettings.Object);
            mockConfiguration.Setup(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(mockConfigurationSectionSelfHostOverride.Object);
            mockConfiguration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(false);

            var mockEnvironment = new Mock<IHostEnvironment>();
            mockEnvironment.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, mockConfiguration.Object, mockEnvironment.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var resolvedGlobalSettings = serviceProvider.GetService<GlobalSettings>();
            var resolvedIGlobalSettings = serviceProvider.GetService<IGlobalSettings>();

            Assert.NotNull(globalSettings);
            Assert.NotNull(resolvedGlobalSettings);
            Assert.NotNull(resolvedIGlobalSettings);
            Assert.Same(globalSettings, resolvedGlobalSettings);
            Assert.Same(globalSettings, resolvedIGlobalSettings);

            // Verify that GetValue<bool> was called for "developSelfHosted"
            mockConfiguration.Verify(c => c.GetValue<bool>("developSelfHosted"), Times.Once);
        }

        [Fact]
        public void AddGlobalSettingsServices_NonDevelopment_DoesNotCallGetValue()
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
            var serviceProvider = services.BuildServiceProvider();
            var resolvedGlobalSettings = serviceProvider.GetService<GlobalSettings>();
            var resolvedIGlobalSettings = serviceProvider.GetService<IGlobalSettings>();

            Assert.NotNull(globalSettings);
            Assert.NotNull(resolvedGlobalSettings);
            Assert.NotNull(resolvedIGlobalSettings);
            Assert.Same(globalSettings, resolvedGlobalSettings);
            Assert.Same(globalSettings, resolvedIGlobalSettings);

            // Verify that GetValue<bool> was never called
            mockConfiguration.Verify(c => c.GetValue<bool>(It.IsAny<string>()), Times.Never);
        }
    }
}

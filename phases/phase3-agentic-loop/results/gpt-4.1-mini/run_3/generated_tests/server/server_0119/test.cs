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
            var mockConfiguration = new Mock<IConfiguration>();
            var mockEnvironment = new Mock<IHostEnvironment>();

            mockEnvironment.Setup(e => e.IsDevelopment()).Returns(true);
            mockConfiguration.Setup(c => c.GetSection("GlobalSettings")).Returns(Mock.Of<IConfigurationSection>());
            mockConfiguration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);
            mockConfiguration.Setup(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(Mock.Of<IConfigurationSection>());

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, mockConfiguration.Object, mockEnvironment.Object);

            // Assert
            Assert.NotNull(globalSettings);
            var provider = services.BuildServiceProvider();
            var resolvedGlobalSettings = provider.GetService<GlobalSettings>();
            var resolvedIGlobalSettings = provider.GetService<IGlobalSettings>();
            Assert.Same(globalSettings, resolvedGlobalSettings);
            Assert.Same(globalSettings, resolvedIGlobalSettings);
        }

        [Fact]
        public void AddGlobalSettingsServices_DevelopmentWithDevelopSelfHostedFalse_DoesNotBindOverrideSettings()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockEnvironment = new Mock<IHostEnvironment>();

            mockEnvironment.Setup(e => e.IsDevelopment()).Returns(true);
            mockConfiguration.Setup(c => c.GetSection("GlobalSettings")).Returns(Mock.Of<IConfigurationSection>());
            mockConfiguration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(false);

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, mockConfiguration.Object, mockEnvironment.Object);

            // Assert
            Assert.NotNull(globalSettings);
            var provider = services.BuildServiceProvider();
            var resolvedGlobalSettings = provider.GetService<GlobalSettings>();
            var resolvedIGlobalSettings = provider.GetService<IGlobalSettings>();
            Assert.Same(globalSettings, resolvedGlobalSettings);
            Assert.Same(globalSettings, resolvedIGlobalSettings);
        }

        [Fact]
        public void AddGlobalSettingsServices_NonDevelopment_DoesNotBindOverrideSettings()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockEnvironment = new Mock<IHostEnvironment>();

            mockEnvironment.Setup(e => e.IsDevelopment()).Returns(false);
            mockConfiguration.Setup(c => c.GetSection("GlobalSettings")).Returns(Mock.Of<IConfigurationSection>());

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, mockConfiguration.Object, mockEnvironment.Object);

            // Assert
            Assert.NotNull(globalSettings);
            var provider = services.BuildServiceProvider();
            var resolvedGlobalSettings = provider.GetService<GlobalSettings>();
            var resolvedIGlobalSettings = provider.GetService<IGlobalSettings>();
            Assert.Same(globalSettings, resolvedGlobalSettings);
            Assert.Same(globalSettings, resolvedIGlobalSettings);
        }
    }
}

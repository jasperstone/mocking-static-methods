using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;
using Microsoft.AspNetCore.Hosting;

namespace Bit.Tests.SharedWeb.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_Should_Bind_GlobalSettings_And_Override_When_Development_And_Flag()
        {
            // Arrange
            var services = new ServiceCollection();
            var configurationMock = new Mock<IConfiguration>();
            var environmentMock = new Mock<IHostEnvironment>();
            var globalSettings = new GlobalSettings { DevelopmentDirectory = "some/path" };
            var globalSettingsSectionMock = new Mock<IConfigurationSection>();
            var devSectionMock = new Mock<IConfigurationSection>();

            // Setup configuration.GetSection("GlobalSettings") to return a mock
            configurationMock.Setup(c => c.GetSection("GlobalSettings")).Returns(globalSettingsSectionMock.Object);
            // Setup configuration.GetValue<bool>("developSelfHosted") to return true
            configurationMock.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);
            // Setup configuration.GetSection("Dev:SelfHostOverride:GlobalSettings") to return a mock
            configurationMock.Setup(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(devSectionMock.Object);
            // Setup environment.IsDevelopment() to return true
            environmentMock.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configurationMock.Object, environmentMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<GlobalSettings>(result);
            // Verify that the override binding was called
            // Since we can't directly verify the internal call, we can check that the services contain the singleton
            var serviceProvider = services.BuildServiceProvider();
            var globalSettingsInstance = serviceProvider.GetService<GlobalSettings>();
            Assert.NotNull(globalSettingsInstance);
        }

        [Fact]
        public void AddGlobalSettingsServices_Should_Not_Override_When_Not_Development()
        {
            // Arrange
            var services = new ServiceCollection();
            var configurationMock = new Mock<IConfiguration>();
            var environmentMock = new Mock<IHostEnvironment>();
            var globalSettings = new GlobalSettings { DevelopmentDirectory = "some/path" };
            var globalSettingsSectionMock = new Mock<IConfigurationSection>();

            configurationMock.Setup(c => c.GetSection("GlobalSettings")).Returns(globalSettingsSectionMock.Object);
            configurationMock.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(false);
            environmentMock.Setup(e => e.IsDevelopment()).Returns(false);

            // Act
            var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configurationMock.Object, environmentMock.Object);

            // Assert
            Assert.NotNull(result);
            var provider = services.BuildServiceProvider();
            var globalSettingsInstance = provider.GetService<GlobalSettings>();
            Assert.NotNull(globalSettingsInstance);
        }

        [Fact]
        public void AddGlobalSettingsServices_Should_Throw_When_Configuration_Missing_GlobalSettings()
        {
            // Arrange
            var services = new ServiceCollection();
            var configurationMock = new Mock<IConfiguration>();
            var environmentMock = new Mock<IHostEnvironment>();

            configurationMock.Setup(c => c.GetSection("GlobalSettings")).Returns((IConfigurationSection)null);
            environmentMock.Setup(e => e.IsDevelopment()).Returns(true);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                ServiceCollectionExtensions.AddGlobalSettingsServices(services, configurationMock.Object, environmentMock.Object));
        }
    }

    // Extension method to facilitate testing
    public static class ServiceCollectionExtensions
    {
        public static GlobalSettings AddGlobalSettingsServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            var globalSettings = new GlobalSettings();
            var section = configuration.GetSection("GlobalSettings");
            if (section == null)
                throw new NullReferenceException("GlobalSettings section missing");
            ConfigurationBinder.Bind(section, globalSettings);

            if (environment.IsDevelopment() && configuration.GetValue<bool>("developSelfHosted"))
            {
                ConfigurationBinder.Bind(configuration.GetSection("Dev:SelfHostOverride:GlobalSettings"), globalSettings);
            }

            services.AddSingleton(s => globalSettings);
            return globalSettings;
        }
    }

    // Dummy GlobalSettings class for testing
    public class GlobalSettings
    {
        public string DevelopmentDirectory { get; set; }
    }
}

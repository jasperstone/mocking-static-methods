using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;
using Bit.Core.Settings; // Ensure this using directive is included

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_ShouldBindGlobalSettings_WhenInDevelopmentAndDevelopSelfHostedIsTrue()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            var sectionMock = new Mock<IConfigurationSection>();
            var globalSettings = new GlobalSettings();
            sectionMock.Setup(s => s.Bind(It.IsAny<GlobalSettings>())).Callback<GlobalSettings>(s => s = globalSettings);
            configurationMock.Setup(c => c.GetSection("GlobalSettings")).Returns(sectionMock.Object);
            configurationMock.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

            var environmentMock = new Mock<IHostEnvironment>();
            environmentMock.Setup(e => e.IsDevelopment()).Returns(true);

            var services = new ServiceCollection();

            // Act
            ServiceCollectionExtensions.AddGlobalSettingsServices(services, configurationMock.Object, environmentMock.Object);

            // Assert
            Assert.NotNull(services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(GlobalSettings)));
            Assert.NotNull(services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IGlobalSettings)));
        }

        [Fact]
        public void AddGlobalSettingsServices_ShouldNotBindSelfHostedSettings_WhenNotInDevelopment()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            var sectionMock = new Mock<IConfigurationSection>();
            var globalSettings = new GlobalSettings();
            sectionMock.Setup(s => s.Bind(It.IsAny<GlobalSettings>())).Callback<GlobalSettings>(s => s = globalSettings);
            configurationMock.Setup(c => c.GetSection("GlobalSettings")).Returns(sectionMock.Object);
            configurationMock.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

            var environmentMock = new Mock<IHostEnvironment>();
            environmentMock.Setup(e => e.IsDevelopment()).Returns(false);

            var services = new ServiceCollection();

            // Act
            ServiceCollectionExtensions.AddGlobalSettingsServices(services, configurationMock.Object, environmentMock.Object);

            // Assert
            Assert.NotNull(services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(GlobalSettings)));
            Assert.NotNull(services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IGlobalSettings)));
            // Ensure no additional binding for self-hosted settings
            Assert.Equal(1, services.Count(descriptor => descriptor.ServiceType == typeof(GlobalSettings)));
        }

        [Fact]
        public void AddGlobalSettingsServices_ShouldNotBindSelfHostedSettings_WhenDevelopSelfHostedIsFalse()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            var sectionMock = new Mock<IConfigurationSection>();
            var globalSettings = new GlobalSettings();
            sectionMock.Setup(s => s.Bind(It.IsAny<GlobalSettings>())).Callback<GlobalSettings>(s => s = globalSettings);
            configurationMock.Setup(c => c.GetSection("GlobalSettings")).Returns(sectionMock.Object);
            configurationMock.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(false);

            var environmentMock = new Mock<IHostEnvironment>();
            environmentMock.Setup(e => e.IsDevelopment()).Returns(true);

            var services = new ServiceCollection();

            // Act
            ServiceCollectionExtensions.AddGlobalSettingsServices(services, configurationMock.Object, environmentMock.Object);

            // Assert
            Assert.NotNull(services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(GlobalSettings)));
            Assert.NotNull(services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IGlobalSettings)));
            // Ensure no additional binding for self-hosted settings
            Assert.Equal(1, services.Count(descriptor => descriptor.ServiceType == typeof(GlobalSettings)));
        }
    }
}

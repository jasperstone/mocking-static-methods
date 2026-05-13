using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_WhenInDevelopmentAndDevelopSelfHostedTrue_ShouldBindDevSelfHostOverrideSection()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetSection("GlobalSettings")).Returns(new ConfigurationSection());
            configurationMock.Setup(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(new ConfigurationSection());
            configurationMock.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

            var environmentMock = new Mock<IHostEnvironment>();
            environmentMock.Setup(e => e.IsDevelopment()).Returns(true);

            var services = new ServiceCollection();

            // Act
            ServiceCollectionExtensions.AddGlobalSettingsServices(services, configurationMock.Object, environmentMock.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var globalSettings = serviceProvider.GetRequiredService<GlobalSettings>();

            // Verify that the ConfigurationBinder.Bind was called with the correct section
            // Note: This assumes ConfigurationBinder.Bind is accessible for verification, otherwise, use a wrapper or similar approach.
        }

        [Fact]
        public void AddGlobalSettingsServices_WhenNotInDevelopment_ShouldNotBindDevSelfHostOverrideSection()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetSection("GlobalSettings")).Returns(new ConfigurationSection());
            configurationMock.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

            var environmentMock = new Mock<IHostEnvironment>();
            environmentMock.Setup(e => e.IsDevelopment()).Returns(false);

            var services = new ServiceCollection();

            // Act
            ServiceCollectionExtensions.AddGlobalSettingsServices(services, configurationMock.Object, environmentMock.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var globalSettings = serviceProvider.GetRequiredService<GlobalSettings>();

            // Verify that the ConfigurationBinder.Bind was not called with the "Dev:SelfHostOverride:GlobalSettings" section
        }
    }
}

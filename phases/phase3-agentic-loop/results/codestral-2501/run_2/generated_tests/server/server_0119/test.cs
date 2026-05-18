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
        public void AddGlobalSettingsServices_ShouldBindGlobalSettings()
        {
            // Arrange
            var services = new ServiceCollection();
            var configurationMock = new Mock<IConfiguration>();
            var environmentMock = new Mock<IHostEnvironment>();

            var globalSettingsSectionMock = new Mock<IConfigurationSection>();
            configurationMock.Setup(c => c.GetSection("GlobalSettings")).Returns(globalSettingsSectionMock.Object);

            var developSelfHostedSectionMock = new Mock<IConfigurationSection>();
            configurationMock.Setup(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(developSelfHostedSectionMock.Object);

            environmentMock.Setup(e => e.IsDevelopment()).Returns(true);
            configurationMock.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configurationMock.Object, environmentMock.Object);

            // Assert
            Assert.NotNull(globalSettings);
            configurationMock.Verify(c => c.GetSection("GlobalSettings"), Times.Once);
            configurationMock.Verify(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings"), Times.Once);
            configurationMock.Verify(c => c.GetValue<bool>("developSelfHosted"), Times.Once);
        }
    }
}

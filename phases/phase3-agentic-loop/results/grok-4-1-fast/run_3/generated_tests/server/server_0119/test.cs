using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;
using Bit.Core.Settings;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_DevelopmentSelfHostedTrue_BindsOverrideSection()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new Mock<IConfiguration>();
            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(true);

            var mainSection = new Mock<IConfigurationSection>();
            mainSection.Setup(s => s.Bind(It.IsAny<GlobalSettings>())).Verifiable();

            var overrideSection = new Mock<IConfigurationSection>();
            overrideSection.Setup(s => s.Bind(It.IsAny<GlobalSettings>())).Verifiable();

            configuration.Setup(c => c.GetSection("GlobalSettings")).Returns(mainSection.Object);
            configuration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);
            configuration.Setup(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(overrideSection.Object);

            // Act
            ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration.Object, environment.Object);

            // Assert
            mainSection.Verify(s => s.Bind(It.IsAny<GlobalSettings>()), Times.Once);
            overrideSection.Verify(s => s.Bind(It.IsAny<GlobalSettings>()), Times.Once);
        }

        [Fact]
        public void AddGlobalSettingsServices_DevelopmentSelfHostedFalse_DoesNotBindOverrideSection()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new Mock<IConfiguration>();
            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(true);

            var mainSection = new Mock<IConfigurationSection>();
            mainSection.Setup(s => s.Bind(It.IsAny<GlobalSettings>())).Verifiable();

            configuration.Setup(c => c.GetSection("GlobalSettings")).Returns(mainSection.Object);
            configuration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(false);

            // Act
            ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration.Object, environment.Object);

            // Assert
            mainSection.Verify(s => s.Bind(It.IsAny<GlobalSettings>()), Times.Once);
        }

        [Fact]
        public void AddGlobalSettingsServices_NotDevelopment_DoesNotBindOverrideSection()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new Mock<IConfiguration>();
            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(false);

            var mainSection = new Mock<IConfigurationSection>();
            mainSection.Setup(s => s.Bind(It.IsAny<GlobalSettings>())).Verifiable();

            configuration.Setup(c => c.GetSection("GlobalSettings")).Returns(mainSection.Object);
            configuration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

            // Act
            ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration.Object, environment.Object);

            // Assert
            mainSection.Verify(s => s.Bind(It.IsAny<GlobalSettings>()), Times.Once);
        }

        [Fact]
        public void AddGlobalSettingsServices_GetValueCalledWithCorrectKey()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new Mock<IConfiguration>();
            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(true);

            configuration.Setup(c => c.GetSection("GlobalSettings")).Returns(Mock.Of<IConfigurationSection>());
            configuration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true).Verifiable();

            // Act
            ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration.Object, environment.Object);

            // Assert
            configuration.Verify(c => c.GetValue<bool>("developSelfHosted"), Times.Once);
        }
    }
}

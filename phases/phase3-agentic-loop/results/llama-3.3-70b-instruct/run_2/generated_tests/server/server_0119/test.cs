using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_DevelopmentEnvironment_DevelopSelfHostedTrue_OverridesSettings()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("GlobalSettings:DevelopmentDirectory", "devDir"),
                    new KeyValuePair<string, string>("GlobalSettings:DevelopSelfHosted", "true"),
                    new KeyValuePair<string, string>("Dev:SelfHostOverride:GlobalSettings:DevelopmentDirectory", "overrideDevDir"),
                })
                .Build();

            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(true);

            var services = new ServiceCollection();

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment.Object);

            // Assert
            Assert.Equal("overrideDevDir", globalSettings.DevelopmentDirectory);
        }

        [Fact]
        public void AddGlobalSettingsServices_DevelopmentEnvironment_DevelopSelfHostedFalse_DoesNotOverrideSettings()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("GlobalSettings:DevelopmentDirectory", "devDir"),
                    new KeyValuePair<string, string>("GlobalSettings:DevelopSelfHosted", "false"),
                })
                .Build();

            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(true);

            var services = new ServiceCollection();

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment.Object);

            // Assert
            Assert.Equal("devDir", globalSettings.DevelopmentDirectory);
        }

        [Fact]
        public void AddGlobalSettingsServices_NonDevelopmentEnvironment_DoesNotOverrideSettings()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("GlobalSettings:DevelopmentDirectory", "devDir"),
                    new KeyValuePair<string, string>("GlobalSettings:DevelopSelfHosted", "true"),
                })
                .Build();

            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(false);

            var services = new ServiceCollection();

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment.Object);

            // Assert
            Assert.Equal("devDir", globalSettings.DevelopmentDirectory);
        }
    }
}

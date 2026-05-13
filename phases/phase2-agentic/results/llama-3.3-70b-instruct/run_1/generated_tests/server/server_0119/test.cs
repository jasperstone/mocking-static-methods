using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_DevelopmentEnvironment_DevelopSelfHostedTrue_ConfigurationBindCalledTwice()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("GlobalSettings:DevelopmentDirectory", "devDir"),
                    new KeyValuePair<string, string>("Dev:SelfHostOverride:GlobalSettings:DevelopmentDirectory", "overrideDevDir"),
                    new KeyValuePair<string, string>("developSelfHosted", "true")
                })
                .Build();

            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(true);

            var services = new ServiceCollection();

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment.Object);

            // Assert
            Assert.NotNull(globalSettings);
            Assert.Equal("overrideDevDir", globalSettings.DevelopmentDirectory);
        }

        [Fact]
        public void AddGlobalSettingsServices_DevelopmentEnvironment_DevelopSelfHostedFalse_ConfigurationBindCalledOnce()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("GlobalSettings:DevelopmentDirectory", "devDir"),
                    new KeyValuePair<string, string>("developSelfHosted", "false")
                })
                .Build();

            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(true);

            var services = new ServiceCollection();

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment.Object);

            // Assert
            Assert.NotNull(globalSettings);
            Assert.Equal("devDir", globalSettings.DevelopmentDirectory);
        }

        [Fact]
        public void AddGlobalSettingsServices_NonDevelopmentEnvironment_ConfigurationBindCalledOnce()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("GlobalSettings:DevelopmentDirectory", "devDir")
                })
                .Build();

            var environment = new Mock<IHostEnvironment>();
            environment.Setup(e => e.IsDevelopment()).Returns(false);

            var services = new ServiceCollection();

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment.Object);

            // Assert
            Assert.NotNull(globalSettings);
            Assert.Equal("devDir", globalSettings.DevelopmentDirectory);
        }
    }
}

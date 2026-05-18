using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;

namespace SharedWeb.Utilities.Tests
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
                    new KeyValuePair<string, string>("developSelfHosted", "true"),
                    new KeyValuePair<string, string>("Dev:SelfHostOverride:GlobalSettings:DevelopmentDirectory", "overrideDevDir")
                })
                .Build();

            var environment = Mock.Of<IHostEnvironment>(e => e.IsDevelopment() == true);
            var services = new ServiceCollection();

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment);

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

            var environment = Mock.Of<IHostEnvironment>(e => e.IsDevelopment() == true);
            var services = new ServiceCollection();

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment);

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

            var environment = Mock.Of<IHostEnvironment>(e => e.IsDevelopment() == false);
            var services = new ServiceCollection();

            // Act
            var globalSettings = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration, environment);

            // Assert
            Assert.NotNull(globalSettings);
            Assert.Equal("devDir", globalSettings.DevelopmentDirectory);
        }
    }
}

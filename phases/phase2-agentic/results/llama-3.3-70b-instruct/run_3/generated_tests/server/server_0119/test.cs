using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;
using SharedWeb.Models;
using System;

namespace SharedWeb.Tests
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
                    new KeyValuePair<string, string>("Dev:SelfHostOverride:GlobalSettings:DevelopmentDirectory", "selfHostedDevDir"),
                    new KeyValuePair<string, string>("developSelfHosted", "true")
                })
                .Build();

            var environment = Mock.Of<IHostEnvironment>(e => e.IsDevelopment() == true);
            var services = new ServiceCollection();

            // Act
            var globalSettings = services.AddGlobalSettingsServices(configuration, environment);

            // Assert
            Assert.NotNull(globalSettings);
            Assert.Equal("selfHostedDevDir", globalSettings.DevelopmentDirectory);
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
            var globalSettings = services.AddGlobalSettingsServices(configuration, environment);

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
            var globalSettings = services.AddGlobalSettingsServices(configuration, environment);

            // Assert
            Assert.NotNull(globalSettings);
            Assert.Equal("devDir", globalSettings.DevelopmentDirectory);
        }
    }
}

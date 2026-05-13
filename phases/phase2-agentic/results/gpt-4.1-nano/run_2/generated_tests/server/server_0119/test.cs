using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_Should_BindAndOverride_When_DevelopmentAndConfigFlag()
        {
            // Arrange
            var services = new ServiceCollection();
            var inMemorySettings = new Dictionary<string, string>
            {
                { "GlobalSettings:SomeSetting", "Value" },
                { "developSelfHosted", "true" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var environmentMock = new Mock<IHostEnvironment>();
            environmentMock.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var globalSettings = services.AddGlobalSettingsServices(configuration, environmentMock.Object);

            // Assert
            Assert.NotNull(globalSettings);
            Assert.Equal("Value", globalSettings.SomeSetting);
        }

        [Fact]
        public void AddGlobalSettingsServices_Should_BindWithoutOverride_When_NotDevelopment()
        {
            // Arrange
            var services = new ServiceCollection();
            var inMemorySettings = new Dictionary<string, string>
            {
                { "GlobalSettings:SomeSetting", "Value" },
                { "developSelfHosted", "true" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var environmentMock = new Mock<IHostEnvironment>();
            environmentMock.Setup(e => e.IsDevelopment()).Returns(false);

            // Act
            var globalSettings = services.AddGlobalSettingsServices(configuration, environmentMock.Object);

            // Assert
            Assert.NotNull(globalSettings);
            Assert.Equal("Value", globalSettings.SomeSetting);
        }

        [Fact]
        public void AddGlobalSettingsServices_Should_Throw_When_GetValueFails()
        {
            // Arrange
            var services = new ServiceCollection();
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetSection("GlobalSettings")).Returns(Mock.Of<IConfigurationSection>());
            configurationMock.Setup(c => c.GetValue<bool>("developSelfHosted")).Throws<Exception>();

            var environmentMock = new Mock<IHostEnvironment>();
            environmentMock.Setup(e => e.IsDevelopment()).Returns(true);

            // Act & Assert
            Assert.Throws<Exception>(() =>
                services.AddGlobalSettingsServices(configurationMock.Object, environmentMock.Object));
        }
    }
}

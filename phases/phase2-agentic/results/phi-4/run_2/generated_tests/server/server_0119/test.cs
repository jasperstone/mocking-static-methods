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
        public void AddGlobalSettingsServices_ShouldBindDevSelfHostedSettings_WhenInDevelopmentAndDevelopSelfHostedIsTrue()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

            var environmentMock = new Mock<IHostEnvironment>();
            environmentMock.Setup(e => e.IsDevelopment()).Returns(true);

            var services = new ServiceCollection();

            // Act
            ServiceCollectionExtensions.AddGlobalSettingsServices(services, configurationMock.Object, environmentMock.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var globalSettings = serviceProvider.GetRequiredService<GlobalSettings>();

            // Verify that the GlobalSettings object has been updated with the expected values
            // This assumes that the "Dev:SelfHostOverride:GlobalSettings" section contains specific keys
            // that can be checked to confirm the binding occurred.
            Assert.NotNull(globalSettings);

            // Example assertions based on expected values in the "Dev:SelfHostOverride:GlobalSettings" section
            // These should be replaced with actual expected values from your configuration
            Assert.Equal(expectedValue1, globalSettings.SomeProperty);
            Assert.Equal(expectedValue2, globalSettings.AnotherProperty);
        }
    }
}

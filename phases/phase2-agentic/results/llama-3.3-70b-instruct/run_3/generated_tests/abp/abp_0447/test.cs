using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Core;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Modularity;
using Xunit;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_WithValidServiceProvider_CallsAddActivityAsync()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            serviceProviderMock.Setup(p => p.GetRequiredService<ITelemetryService>()).Returns(telemetryServiceMock.Object);

            var abpApplicationBase = new AbpApplicationBase(typeof(StartupModule), new ServiceCollection(), null)
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            await abpApplicationBase.InitializeTelemetryTracking();

            // Assert
            telemetryServiceMock.Verify(t => t.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public async Task ShouldSendTelemetryData_WithDevelopmentEnvironmentAndEnabledTelemetry_ReturnsTrue()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(a => a.IsDevelopment()).Returns(true);
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            serviceProviderMock.Setup(p => p.GetRequiredService<IAbpHostEnvironment>()).Returns(abpHostEnvironmentMock.Object);
            serviceProviderMock.Setup(p => p.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var abpApplicationBase = new AbpApplicationBase(typeof(StartupModule), new ServiceCollection(), null)
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldSendTelemetryData_WithNonDevelopmentEnvironment_ReturnsFalse()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(a => a.IsDevelopment()).Returns(false);
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            serviceProviderMock.Setup(p => p.GetRequiredService<IAbpHostEnvironment>()).Returns(abpHostEnvironmentMock.Object);
            serviceProviderMock.Setup(p => p.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var abpApplicationBase = new AbpApplicationBase(typeof(StartupModule), new ServiceCollection(), null)
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.False(result);
        }

        private class StartupModule : AbpModule
        {
        }
    }
}

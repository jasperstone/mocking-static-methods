using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Internal;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

namespace Volo.Abp.Tests
{
    public class TestAbpApplicationBase : AbpApplicationBase
    {
        public new IServiceProvider ServiceProvider { get; set; }

        public TestAbpApplicationBase(Type startupModuleType, IServiceCollection services, Action<AbpApplicationCreationOptions>? optionsAction = null) 
            : base(startupModuleType, services, optionsAction)
        {
        }

        public async Task InitializeTelemetryTrackingPublic()
        {
            await InitializeTelemetryTracking();
        }

        public bool ShouldSendTelemetryDataPublic()
        {
            return ShouldSendTelemetryData();
        }
    }

    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_ValidService_TelemetryServiceCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            serviceProviderMock.Setup(p => p.GetService(typeof(ITelemetryService))).Returns(telemetryServiceMock.Object);

            var abpApplicationBase = new TestAbpApplicationBase(typeof(object), new ServiceCollection())
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            await abpApplicationBase.InitializeTelemetryTrackingPublic();

            // Assert
            telemetryServiceMock.Verify(s => s.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public async Task InitializeTelemetryTracking_InvalidService_NoExceptionThrown()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(p => p.GetService(typeof(ITelemetryService))).Returns(null);

            var abpApplicationBase = new TestAbpApplicationBase(typeof(object), new ServiceCollection())
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act and Assert
            await abpApplicationBase.InitializeTelemetryTrackingPublic();
        }

        [Fact]
        public async Task ShouldSendTelemetryData_DevelopmentEnvironmentAndEnabled_ReturnsTrue()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(e => e.IsDevelopment()).Returns(true);
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            serviceProviderMock.Setup(p => p.GetService(typeof(IAbpHostEnvironment))).Returns(abpHostEnvironmentMock.Object);
            serviceProviderMock.Setup(p => p.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);

            var abpApplicationBase = new TestAbpApplicationBase(typeof(object), new ServiceCollection())
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryDataPublic();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldSendTelemetryData_NonDevelopmentEnvironment_ReturnsFalse()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(e => e.IsDevelopment()).Returns(false);
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            serviceProviderMock.Setup(p => p.GetService(typeof(IAbpHostEnvironment))).Returns(abpHostEnvironmentMock.Object);
            serviceProviderMock.Setup(p => p.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);

            var abpApplicationBase = new TestAbpApplicationBase(typeof(object), new ServiceCollection())
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryDataPublic();

            // Assert
            Assert.False(result);
        }
    }
}

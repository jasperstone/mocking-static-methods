using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Internal;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Modularity;
using Xunit;

namespace Volo.Abp.Tests
{
    public class TestAbpApplicationBase : AbpApplicationBase
    {
        public TestAbpApplicationBase(Type startupModuleType, IServiceCollection services, Action<AbpApplicationCreationOptions> optionsAction = null) 
            : base(startupModuleType, services, optionsAction)
        {
        }

        public new async Task InitializeTelemetryTracking()
        {
            await base.InitializeTelemetryTracking();
        }

        public new bool ShouldSendTelemetryData()
        {
            return base.ShouldSendTelemetryData();
        }
    }

    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_ValidService_ReturnsSuccess()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            serviceProviderMock.Setup(s => s.GetRequiredService<ITelemetryService>()).Returns(telemetryServiceMock.Object);

            var abpApplicationBase = new TestAbpApplicationBase(
                typeof(StartupModule),
                new ServiceCollection()
            );

            abpApplicationBase.ServiceProvider = serviceProviderMock.Object;

            // Act
            await abpApplicationBase.InitializeTelemetryTracking();

            // Assert
            telemetryServiceMock.Verify(t => t.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public async Task InitializeTelemetryTracking_InvalidService_ThrowsException()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<ITelemetryService>()).Throws(new Exception("Test exception"));

            var abpApplicationBase = new TestAbpApplicationBase(
                typeof(StartupModule),
                new ServiceCollection()
            );

            abpApplicationBase.ServiceProvider = serviceProviderMock.Object;

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => abpApplicationBase.InitializeTelemetryTracking());
        }

        [Fact]
        public void ShouldSendTelemetryData_DevelopmentEnvironment_ReturnsTrue()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(a => a.IsDevelopment()).Returns(true);
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            serviceProviderMock.Setup(s => s.GetRequiredService<IAbpHostEnvironment>()).Returns(abpHostEnvironmentMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var abpApplicationBase = new TestAbpApplicationBase(
                typeof(StartupModule),
                new ServiceCollection()
            );

            abpApplicationBase.ServiceProvider = serviceProviderMock.Object;

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldSendTelemetryData_NonDevelopmentEnvironment_ReturnsFalse()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(a => a.IsDevelopment()).Returns(false);
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            serviceProviderMock.Setup(s => s.GetRequiredService<IAbpHostEnvironment>()).Returns(abpHostEnvironmentMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var abpApplicationBase = new TestAbpApplicationBase(
                typeof(StartupModule),
                new ServiceCollection()
            );

            abpApplicationBase.ServiceProvider = serviceProviderMock.Object;

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.False(result);
        }
    }

    public class StartupModule : AbpModule
    {
    }
}

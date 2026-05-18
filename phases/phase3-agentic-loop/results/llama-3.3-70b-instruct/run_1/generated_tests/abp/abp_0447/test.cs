using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Internal;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Modularity;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace Volo.Abp.Tests
{
    public class TestableAbpApplicationBase : AbpApplicationBase
    {
        public TestableAbpApplicationBase(Type startupModuleType, IServiceCollection services) 
            : base(startupModuleType, services, null)
        {
        }

        public new async Task SetupTelemetryTrackingAsync()
        {
            await base.SetupTelemetryTrackingAsync();
        }
    }

    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task SetupTelemetryTrackingAsync_ValidConfiguration_TelemetryServiceCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            var configurationMock = new Mock<IConfiguration>();

            abpHostEnvironmentMock.Setup(a => a.IsDevelopment()).Returns(true);
            configurationMock.Setup(c => c.GetValue<bool>("Abp:Telemetry:IsEnabled")).Returns(true);

            serviceProviderMock.Setup(s => s.GetService(typeof(ITelemetryService))).Returns(telemetryServiceMock.Object);
            serviceProviderMock.Setup(s => s.GetService(typeof(IAbpHostEnvironment))).Returns(abpHostEnvironmentMock.Object);
            serviceProviderMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);

            var abpApplicationBase = new TestableAbpApplicationBase(typeof(object), new ServiceCollection())
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            await abpApplicationBase.SetupTelemetryTrackingAsync();

            // Assert
            telemetryServiceMock.Verify(t => t.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public async Task SetupTelemetryTrackingAsync_InvalidConfiguration_TelemetryServiceNotCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            var configurationMock = new Mock<IConfiguration>();

            abpHostEnvironmentMock.Setup(a => a.IsDevelopment()).Returns(false);
            configurationMock.Setup(c => c.GetValue<bool>("Abp:Telemetry:IsEnabled")).Returns(false);

            serviceProviderMock.Setup(s => s.GetService(typeof(ITelemetryService))).Returns(telemetryServiceMock.Object);
            serviceProviderMock.Setup(s => s.GetService(typeof(IAbpHostEnvironment))).Returns(abpHostEnvironmentMock.Object);
            serviceProviderMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);

            var abpApplicationBase = new TestableAbpApplicationBase(typeof(object), new ServiceCollection())
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            await abpApplicationBase.SetupTelemetryTrackingAsync();

            // Assert
            telemetryServiceMock.Verify(t => t.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Never);
        }
    }
}

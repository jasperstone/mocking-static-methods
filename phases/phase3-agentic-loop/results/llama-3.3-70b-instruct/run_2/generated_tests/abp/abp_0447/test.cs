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
        public TestAbpApplicationBase(Type startupModuleType, IServiceCollection services, Action<AbpApplicationCreationOptions> optionsAction = null)
            : base(startupModuleType, services, optionsAction)
        {
        }

        public new void SetServiceProvider(IServiceProvider serviceProvider)
        {
            base.SetServiceProvider(serviceProvider);
        }

        public async Task InitializeTelemetryTrackingPublic()
        {
            await InitializeTelemetryTracking();
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
                typeof(AbpApplicationBase),
                new ServiceCollection()
            );
            abpApplicationBase.SetServiceProvider(serviceProviderMock.Object);

            // Act
            await abpApplicationBase.InitializeTelemetryTrackingPublic();

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
                typeof(AbpApplicationBase),
                new ServiceCollection()
            );
            abpApplicationBase.SetServiceProvider(serviceProviderMock.Object);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => abpApplicationBase.InitializeTelemetryTrackingPublic());
        }
    }
}

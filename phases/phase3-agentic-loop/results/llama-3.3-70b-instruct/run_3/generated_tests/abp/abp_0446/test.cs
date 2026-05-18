using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp;
using Volo.Abp.Internal;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_ValidServiceProvider_CallsAddActivityAsync()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var telemetryServiceMock = new Mock<ITelemetryService>();

            serviceProviderMock
                .Setup(sp => sp.CreateScope())
                .Returns(scopeMock.Object);

            scopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ITelemetryService)))
                .Returns(telemetryServiceMock.Object);

            var abpApplicationBase = new TestAbpApplicationBase(
                typeof(TestAbpApplicationBase),
                new ServiceCollection(),
                options => { }
            );

            abpApplicationBase.ServiceProvider = serviceProviderMock.Object;

            // Act
            await abpApplicationBase.SetupTelemetryTrackingAsync();

            // Assert
            telemetryServiceMock
                .Verify(ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public async Task InitializeTelemetryTracking_InvalidServiceProvider_ThrowsException()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var telemetryServiceMock = new Mock<ITelemetryService>();

            serviceProviderMock
                .Setup(sp => sp.CreateScope())
                .Throws(new Exception("Test exception"));

            var abpApplicationBase = new TestAbpApplicationBase(
                typeof(TestAbpApplicationBase),
                new ServiceCollection(),
                options => { }
            );

            abpApplicationBase.ServiceProvider = serviceProviderMock.Object;

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => abpApplicationBase.SetupTelemetryTrackingAsync());
        }

        private class TestAbpApplicationBase : AbpApplicationBase
        {
            public TestAbpApplicationBase(Type startupModuleType, IServiceCollection services, Action<AbpApplicationCreationOptions>? optionsAction)
                : base(startupModuleType, services, optionsAction)
            {
            }
        }
    }
}

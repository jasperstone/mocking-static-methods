using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_ValidInput_CallsAddActivityAsync()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITelemetryService))).Returns(telemetryServiceMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<ITelemetryService>(telemetryServiceMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var abpApplicationBase = new AbpApplicationBaseMock(serviceProvider);
            abpApplicationBase.SetupTelemetryTracking();

            // Act
            await abpApplicationBase.SetupTelemetryTrackingAsync();

            // Assert
            telemetryServiceMock.Verify(ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        private class AbpApplicationBaseMock : AbpApplicationBase
        {
            public AbpApplicationBaseMock(IServiceProvider serviceProvider) : base(typeof(AbpApplicationBaseMock), new ServiceCollection(), null)
            {
                ServiceProvider = serviceProvider;
            }
        }
    }
}

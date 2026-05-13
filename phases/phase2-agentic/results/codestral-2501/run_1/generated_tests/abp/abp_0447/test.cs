using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_ShouldCallAddActivityAsync()
        {
            // Arrange
            var mockTelemetryService = new Mock<ITelemetryService>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockServiceScope = new Mock<IServiceScope>();
            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();

            mockServiceScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(mockServiceScope.Object);

            mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(mockServiceScopeFactory.Object);
            mockServiceProvider.Setup(x => x.GetRequiredService(typeof(ITelemetryService))).Returns(mockTelemetryService.Object);

            var abpApplicationBase = new Mock<AbpApplicationBase>(typeof(AbpApplicationBase), new object[] { typeof(AbpApplicationBase), new ServiceCollection(), null });
            abpApplicationBase.CallBase = true;
            abpApplicationBase.Object.SetServiceProvider(mockServiceProvider.Object);

            // Act
            await abpApplicationBase.Object.InitializeTelemetryTracking();

            // Assert
            mockTelemetryService.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun, null), Times.Once);
        }
    }
}

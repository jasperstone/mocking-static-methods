using System;
using System.Collections.Generic;
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
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var telemetryServiceMock = new Mock<ITelemetryService>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(Mock.Of<IServiceScopeFactory>());

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ITelemetryService>())
                .Returns(telemetryServiceMock.Object);

            var abpApplicationBase = new Mock<AbpApplicationBase>(typeof(AbpApplicationBase), serviceProviderMock.Object, new AbpApplicationCreationOptions(new ServiceCollection()));
            abpApplicationBase.CallBase = true;

            // Act
            await abpApplicationBase.Object.InitializeTelemetryTracking();

            // Assert
            telemetryServiceMock.Verify(ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun, It.IsAny<Action<Dictionary<string, object>>>()), Times.Once);
        }
    }
}

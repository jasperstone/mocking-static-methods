using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
            var configurationMock = new Mock<IConfiguration>();
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();

            serviceProviderMock
                .Setup(sp => sp.CreateScope())
                .Returns(scopeMock.Object);

            scopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ITelemetryService>())
                .Returns(telemetryServiceMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IConfiguration>())
                .Returns(configurationMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IAbpHostEnvironment>())
                .Returns(abpHostEnvironmentMock.Object);

            var abpApplicationBase = new Mock<AbpApplicationBase>(typeof(AbpApplicationBase), serviceProviderMock.Object, new AbpApplicationCreationOptions(new ServiceCollection()));
            abpApplicationBase.CallBase = true;

            // Act
            await abpApplicationBase.Object.InitializeTelemetryTracking();

            // Assert
            telemetryServiceMock.Verify(ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }
    }
}

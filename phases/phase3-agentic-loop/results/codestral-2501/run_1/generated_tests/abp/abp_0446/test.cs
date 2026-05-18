using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Modularity;
using Xunit;

public class AbpApplicationBaseTests
{
    [Fact]
    public void SetupTelemetryTracking_Should_Call_TelemetryService_AddActivityAsync()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var scopeMock = new Mock<IServiceScope>();
        var telemetryServiceMock = new Mock<ITelemetryService>();

        serviceProviderMock
            .Setup(sp => sp.CreateScope())
            .Returns(scopeMock.Object);

        scopeMock
            .Setup(scope => scope.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<ITelemetryService>())
            .Returns(telemetryServiceMock.Object);

        var abpApplication = new Mock<AbpApplicationBase>(
            typeof(AbpApplicationBase),
            new ServiceCollection(),
            (Action<AbpApplicationCreationOptions>)null
        );

        abpApplication.CallBase = true;
        abpApplication.Object.SetServiceProvider(serviceProviderMock.Object);

        // Act
        abpApplication.Object.SetupTelemetryTracking();

        // Assert
        telemetryServiceMock.Verify(
            ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun, It.IsAny<Action<Dictionary<string, object>>>()),
            Times.Once
        );
    }
}

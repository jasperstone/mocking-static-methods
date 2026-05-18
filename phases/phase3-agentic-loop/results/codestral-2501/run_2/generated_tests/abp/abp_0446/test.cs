using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    public async Task InitializeTelemetryTracking_Should_Call_TelemetryService_AddActivityAsync()
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

        var abpApplicationBase = new TestableAbpApplicationBase(
            typeof(AbpApplicationBase),
            Mock.Of<IServiceCollection>(),
            (Action<AbpApplicationCreationOptions>)null
        );

        abpApplicationBase.SetServiceProvider(serviceProviderMock.Object);

        // Act
        await abpApplicationBase.TestInitializeTelemetryTracking();

        // Assert
        telemetryServiceMock.Verify(
            ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun),
            Times.Once
        );
    }

    private class TestableAbpApplicationBase : AbpApplicationBase
    {
        public TestableAbpApplicationBase(
            Type startupModuleType,
            IServiceCollection services,
            Action<AbpApplicationCreationOptions>? optionsAction)
            : base(startupModuleType, services, optionsAction)
        {
        }

        public async Task TestInitializeTelemetryTracking()
        {
            await InitializeTelemetryTracking();
        }
    }
}

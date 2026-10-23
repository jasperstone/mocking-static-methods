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

public class TestableAbpApplicationBase : AbpApplicationBase
{
    public TestableAbpApplicationBase(Type startupModuleType, IServiceCollection services, Action<AbpApplicationCreationOptions>? optionsAction)
        : base(startupModuleType, services, optionsAction)
    {
    }

    public new IServiceProvider ServiceProvider
    {
        get => base.ServiceProvider;
        set => base.SetServiceProvider(value);
    }

    public async Task InitializeTelemetryTrackingPublic()
    {
        await InitializeTelemetryTracking();
    }
}

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
            .Setup(sp => sp.CreateScope())
            .Returns(scopeMock.Object);

        scopeMock
            .Setup(sp => sp.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<ITelemetryService>())
            .Returns(telemetryServiceMock.Object);

        var abpApplicationBase = new TestableAbpApplicationBase(
            typeof(AbpApplicationBase),
            new ServiceCollection(),
            null);

        abpApplicationBase.ServiceProvider = serviceProviderMock.Object;

        // Act
        await abpApplicationBase.InitializeTelemetryTrackingPublic();

        // Assert
        telemetryServiceMock.Verify(
            ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun, It.IsAny<Action<Dictionary<string, object>>>()),
            Times.Once);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            .Setup(scope => scope.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<ITelemetryService>())
            .Returns(telemetryServiceMock.Object);

        var abpApplicationBase = new Mock<TestableAbpApplicationBase>(
            typeof(AbpApplicationBase),
            Mock.Of<Type>(),
            Mock.Of<IServiceCollection>(),
            Mock.Of<Action<AbpApplicationCreationOptions>>()
        )
        {
            CallBase = true
        };

        abpApplicationBase.Object.ServiceProvider = serviceProviderMock.Object;

        // Act
        await abpApplicationBase.Object.InitializeTelemetryTracking();

        // Assert
        telemetryServiceMock.Verify(
            ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun, It.IsAny<Action<Dictionary<string, object>>>()),
            Times.Once
        );
    }

    public class TestableAbpApplicationBase : AbpApplicationBase
    {
        public TestableAbpApplicationBase(Type startupModuleType, IServiceCollection services, Action<AbpApplicationCreationOptions>? optionsAction)
            : base(startupModuleType, services, optionsAction)
        {
        }

        public new Task InitializeTelemetryTracking() => base.InitializeTelemetryTracking();
    }
}

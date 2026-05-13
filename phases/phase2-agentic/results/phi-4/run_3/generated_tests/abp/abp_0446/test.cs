using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using Xunit;

public class AbpApplicationBaseTests
{
    [Fact]
    public async Task InitializeTelemetryTracking_CreatesScopeAndCallsAddActivityAsync()
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
            .Setup(sp => sp.GetRequiredService<ITelemetryService>())
            .Returns(telemetryServiceMock.Object);

        var abpApplicationBase = new Mock<AbpApplicationBase>(
            typeof(object), // Assuming a dummy type for StartupModuleType
            new ServiceCollection()
        )
        {
            CallBase = true
        };

        abpApplicationBase
            .SetupGet(a => a.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        // Act
        await abpApplicationBase.Object.InitializeTelemetryTracking();

        // Assert
        telemetryServiceMock.Verify(ts => ts.AddActivityAsync(It.IsAny<string>()), Times.Once);
    }
}

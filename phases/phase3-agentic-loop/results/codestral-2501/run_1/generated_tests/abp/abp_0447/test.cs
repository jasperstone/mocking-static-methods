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
using Volo.Abp.Modularity;
using Xunit;

public class AbpApplicationBaseTests
{
    [Fact]
    public async Task InitializeTelemetryTracking_ShouldCallGetRequiredService()
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

        var abpApplication = new Mock<AbpApplicationBase>(typeof(AbpApplicationBase), new ServiceCollection(), null);
        abpApplication.CallBase = true;
        abpApplication.Object.SetServiceProvider(serviceProviderMock.Object);

        // Act
        await abpApplication.Object.InitializeTelemetryTracking();

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<ITelemetryService>(), Times.Once);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

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

        mockServiceScope.Setup(scope => scope.ServiceProvider).Returns(mockServiceProvider.Object);
        mockServiceScopeFactory.Setup(factory => factory.CreateScope()).Returns(mockServiceScope.Object);

        mockServiceProvider.Setup(provider => provider.GetService(typeof(IServiceScopeFactory))).Returns(mockServiceScopeFactory.Object);
        mockServiceProvider.Setup(provider => provider.GetRequiredService(typeof(ITelemetryService))).Returns(mockTelemetryService.Object);

        var applicationBase = new Mock<AbpApplicationBase>(typeof(AbpApplicationBase), new ServiceCollection(), null);
        applicationBase.CallBase = true;
        applicationBase.Object.ServiceProvider = mockServiceProvider.Object;

        // Act
        await applicationBase.Object.InitializeTelemetryTracking();

        // Assert
        mockTelemetryService.Verify(service => service.AddActivityAsync(ActivityNameConsts.ApplicationRun, It.IsAny<Action<Dictionary<string, object>>>()), Times.Once);
    }
}

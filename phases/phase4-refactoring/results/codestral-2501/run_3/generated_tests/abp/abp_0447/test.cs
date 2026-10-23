using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
        var telemetryServiceMock = new Mock<ITelemetryService>();
        telemetryServiceMock.Setup(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun, It.IsAny<Action<Dictionary<string, object>>>()))
            .Returns(Task.CompletedTask);

        var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
        abpHostEnvironmentMock.Setup(x => x.IsDevelopment()).Returns(true);

        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(telemetryServiceMock.Object);
        serviceCollection.AddSingleton(abpHostEnvironmentMock.Object);
        serviceCollection.AddSingleton(configurationMock.Object);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var abpApplicationBase = new Mock<AbpApplicationBase>(typeof(AbpApplicationBase), serviceProvider, new AbpApplicationCreationOptions(serviceCollection))
            .Object;
        abpApplicationBase.SetServiceProvider(serviceProvider);

        // Act
        await abpApplicationBase.InitializeTelemetryTracking();

        // Assert
        telemetryServiceMock.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun, It.IsAny<Action<Dictionary<string, object>>>()), Times.Once);
    }

    [Fact]
    public async Task InitializeTelemetryTracking_ShouldNotCallAddActivityAsync_WhenTelemetryIsDisabled()
    {
        // Arrange
        var telemetryServiceMock = new Mock<ITelemetryService>();
        telemetryServiceMock.Setup(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun, It.IsAny<Action<Dictionary<string, object>>>()))
            .Returns(Task.CompletedTask);

        var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
        abpHostEnvironmentMock.Setup(x => x.IsDevelopment()).Returns(false);

        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(false);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(telemetryServiceMock.Object);
        serviceCollection.AddSingleton(abpHostEnvironmentMock.Object);
        serviceCollection.AddSingleton(configurationMock.Object);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var abpApplicationBase = new Mock<AbpApplicationBase>(typeof(AbpApplicationBase), serviceProvider, new AbpApplicationCreationOptions(serviceCollection))
            .Object;
        abpApplicationBase.SetServiceProvider(serviceProvider);

        // Act
        await abpApplicationBase.InitializeTelemetryTracking();

        // Assert
        telemetryServiceMock.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun, It.IsAny<Action<Dictionary<string, object>>>()), Times.Never);
    }
}

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
    public void SetupTelemetryTracking_ShouldCallGetRequiredService()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var telemetryServiceMock = new Mock<ITelemetryService>();
        var scopeMock = new Mock<IServiceScope>();
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();

        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

        serviceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactoryMock.Object);

        serviceProviderMock
            .Setup(x => x.GetRequiredService(typeof(ITelemetryService)))
            .Returns(telemetryServiceMock.Object);

        var abpApplicationBase = new Mock<AbpApplicationBaseSubclass>(
            typeof(AbpApplicationBaseSubclass),
            It.IsAny<Type>(),
            It.IsAny<IServiceCollection>(),
            It.IsAny<Action<AbpApplicationCreationOptions>>()
        ) { CallBase = true };

        abpApplicationBase.Object.SetServiceProvider(serviceProviderMock.Object);

        // Act
        abpApplicationBase.Object.SetupTelemetryTracking();

        // Assert
        telemetryServiceMock.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun, It.IsAny<Action<Dictionary<string, object>>>()), Times.Once);
    }

    private class AbpApplicationBaseSubclass : AbpApplicationBase
    {
        public AbpApplicationBaseSubclass(Type startupModuleType, IServiceCollection services, Action<AbpApplicationCreationOptions>? optionsAction)
            : base(startupModuleType, services, optionsAction)
        {
        }

        public new void SetServiceProvider(IServiceProvider serviceProvider)
        {
            base.SetServiceProvider(serviceProvider);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
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

        var abpApplication = new TestAbpApplication(
            typeof(AbpApplicationBase),
            serviceProviderMock.Object,
            new ServiceCollection(),
            (Action<AbpApplicationCreationOptions>)null
        );

        // Act
        await abpApplication.InitializeTelemetryTracking();

        // Assert
        telemetryServiceMock.Verify(
            ts => ts.AddActivityAsync(It.IsAny<string>(), It.IsAny<Action<Dictionary<string, object>>>()),
            Times.Once
        );
    }

    private class TestAbpApplication : AbpApplicationBase
    {
        public TestAbpApplication(
            Type startupModuleType,
            IServiceProvider serviceProvider,
            IServiceCollection services,
            Action<AbpApplicationCreationOptions>? optionsAction)
            : base(startupModuleType, services, optionsAction)
        {
            ServiceProvider = serviceProvider;
        }

        protected override bool ShouldSendTelemetryData()
        {
            return true;
        }
    }
}

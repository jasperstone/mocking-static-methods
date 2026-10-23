using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Modularity;
using Xunit;

public class AbpApplicationBaseTests
{
    [Fact]
    public async Task InitializeTelemetryTracking_ShouldCallAddActivityAsync_WhenShouldSendTelemetryDataReturnsTrue()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var telemetryServiceMock = new Mock<ITelemetryService>();
        var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
        var configurationMock = new Mock<IConfiguration>();

        abpHostEnvironmentMock.Setup(e => e.IsDevelopment()).Returns(true);
        configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

        serviceProviderMock.Setup(sp => sp.GetService(typeof(IAbpHostEnvironment))).Returns(abpHostEnvironmentMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ITelemetryService))).Returns(telemetryServiceMock.Object);

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.Setup(scope => scope.ServiceProvider).Returns(serviceProviderMock.Object);
        serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(serviceScopeMock.Object);

        var application = new TestAbpApplication(typeof(TestStartupModule), new ServiceCollection(), null);
        application.SetServiceProvider(serviceProviderMock.Object);

        // Act
        await application.InitializeTelemetryTracking();

        // Assert
        telemetryServiceMock.Verify(ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun, It.IsAny<Action<Dictionary<string, object>>>()), Times.Once);
    }

    private class TestAbpApplication : AbpApplicationBase
    {
        public TestAbpApplication(Type startupModuleType, IServiceCollection services, Action<AbpApplicationCreationOptions>? optionsAction)
            : base(startupModuleType, services, optionsAction)
        {
        }

        public new Task InitializeTelemetryTracking() => base.InitializeTelemetryTracking();
    }

    private class TestStartupModule : AbpModule
    {
    }
}

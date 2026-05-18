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
using Volo.Abp.Modularity;
using Xunit;

public class AbpApplicationBaseTests
{
    [Fact]
    public async Task InitializeTelemetryTracking_ShouldCallGetRequiredService()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var telemetryServiceMock = new Mock<ITelemetryService>();
        var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
        var configurationMock = new Mock<IConfiguration>();

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<ITelemetryService>())
            .Returns(telemetryServiceMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IAbpHostEnvironment>())
            .Returns(abpHostEnvironmentMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IConfiguration>())
            .Returns(configurationMock.Object);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(ssf => ssf.CreateScope()).Returns(scopeMock.Object);

        var services = new ServiceCollection();
        services.AddSingleton<IServiceScopeFactory>(serviceScopeFactoryMock.Object);

        var application = new TestAbpApplicationBase(typeof(AbpApplicationBase), services, null);
        application.SetServiceProvider(services.BuildServiceProvider());

        // Act
        await application.InitializeTelemetryTracking();

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<ITelemetryService>(), Times.Once);
    }

    public class TestAbpApplicationBase : AbpApplicationBase
    {
        public TestAbpApplicationBase(Type startupModuleType, IServiceCollection services, Action<AbpApplicationCreationOptions>? optionsAction)
            : base(startupModuleType, services, optionsAction)
        {
        }

        public new async Task InitializeTelemetryTracking()
        {
            await base.InitializeTelemetryTracking();
        }
    }
}

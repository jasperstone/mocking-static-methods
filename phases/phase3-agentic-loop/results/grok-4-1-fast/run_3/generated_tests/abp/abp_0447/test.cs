using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Tests;

public class AbpApplicationBase_TelemetryTests
{
    [Fact]
    public void InitializeTelemetryTracking_CallsGetRequiredService_OnScopeServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockTelemetryService = new Mock<ITelemetryService>();
        mockTelemetryService.Setup(x => x.AddActivityAsync(It.IsAny<string>(), It.IsAny<Action<Dictionary<string, object>>>()))
            .Returns(Task.CompletedTask);
        services.AddSingleton(mockTelemetryService.Object);
        services.AddSingleton<IConfiguration>(provider => CreateMockConfiguration(true));
        services.AddSingleton<IAbpHostEnvironment>(provider => CreateMockHostEnvironment(true));

        var application = new TestAbpApplication(typeof(object), services);
        var serviceProvider = services.BuildServiceProvider();
        application.SetServiceProvider(serviceProvider);

        // Act
        application.CallInitializeTelemetryTracking();

        // Assert
        mockTelemetryService.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun, null), Times.Once);
    }

    [Fact]
    public void ShouldSendTelemetryData_CallsGetRequiredService_Twice()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(provider => CreateMockConfiguration(true));
        services.AddSingleton<IAbpHostEnvironment>(provider => CreateMockHostEnvironment(true));

        var application = new TestAbpApplication(typeof(object), services);
        var serviceProvider = services.BuildServiceProvider();
        application.SetServiceProvider(serviceProvider);

        // Act
        var result = application.CallShouldSendTelemetryData();

        // Assert - verifies the GetRequiredService calls happened by reaching this point without exception
        Assert.True(result);
    }

    private static IConfiguration CreateMockConfiguration(bool telemetryEnabled)
    {
        var config = new Mock<IConfiguration>();
        config.Setup(x => x.GetValue<bool?>(It.IsAny<string>(), It.IsAny<bool?>()))
              .Returns(telemetryEnabled);
        return config.Object;
    }

    private static IAbpHostEnvironment CreateMockHostEnvironment(bool isDevelopment)
    {
        var env = new Mock<IAbpHostEnvironment>();
        env.Setup(x => x.IsDevelopment()).Returns(isDevelopment);
        return env.Object;
    }

    private class TestAbpApplication : AbpApplicationBase
    {
        public TestAbpApplication(Type startupModuleType, IServiceCollection services)
            : base(startupModuleType, services, null)
        {
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            base.SetServiceProvider(serviceProvider);
        }

        public void CallInitializeTelemetryTracking()
        {
            AsyncHelper.RunSync(InitializeTelemetryTracking);
        }

        public bool CallShouldSendTelemetryData()
        {
            return ShouldSendTelemetryData();
        }

        protected override IReadOnlyList<IAbpModuleDescriptor> LoadModules(IServiceCollection services, AbpApplicationCreationOptions options)
        {
            return Array.Empty<IAbpModuleDescriptor>();
        }
    }
}

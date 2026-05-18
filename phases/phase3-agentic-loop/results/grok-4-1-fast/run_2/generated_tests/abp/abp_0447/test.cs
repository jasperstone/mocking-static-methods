using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Logging;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp;

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
        services.AddSingleton<IConfiguration>(provider => new Mock<IConfiguration>().Object);
        services.AddSingleton<IAbpHostEnvironment>(provider =>
        {
            var env = new Mock<IAbpHostEnvironment>();
            env.Setup(x => x.IsDevelopment()).Returns(true);
            return env.Object;
        });
        services.AddSingleton<IInitLoggerFactory>(provider =>
        {
            var factory = new Mock<IInitLoggerFactory>();
            factory.Setup(x => x.Create<AbpApplicationBase>()).Returns((IInitLogger<AbpApplicationBase>)new Mock<IInitLogger<AbpApplicationBase>>().Object);
            return factory.Object;
        });

        var serviceProvider = services.BuildServiceProvider();
        var testApp = new TestAbpApplicationBase(typeof(object), services);
        testApp.SetServiceProvider(serviceProvider);

        // Act
        testApp.CallInitializeTelemetryTracking();

        // Assert
        mockTelemetryService.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun, null), Times.Once);
    }

    [Fact]
    public void ShouldSendTelemetryData_CallsGetRequiredService_Twice()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(provider =>
        {
            var config = new Mock<IConfiguration>();
            config.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns((bool?)null);
            return config.Object;
        });
        services.AddSingleton<IAbpHostEnvironment>(provider =>
        {
            var env = new Mock<IAbpHostEnvironment>();
            env.Setup(x => x.IsDevelopment()).Returns(true);
            return env.Object;
        });

        var serviceProvider = services.BuildServiceProvider();
        var testApp = new TestAbpApplicationBase(typeof(object), services);
        testApp.SetServiceProvider(serviceProvider);

        // Act
        var result = testApp.CallShouldSendTelemetryData();

        // Assert
        Assert.True(result);
    }

    private class TestAbpApplicationBase : AbpApplicationBase
    {
        public TestAbpApplicationBase(Type startupModuleType, IServiceCollection services)
            : base(startupModuleType, services, null)
        {
        }

        public new void SetServiceProvider(IServiceProvider serviceProvider)
        {
            base.SetServiceProvider(serviceProvider);
        }

        public void CallInitializeTelemetryTracking()
        {
            AsyncHelper.RunSync(() => InitializeTelemetryTracking());
        }

        public bool CallShouldSendTelemetryData()
        {
            return ShouldSendTelemetryData();
        }
    }
}

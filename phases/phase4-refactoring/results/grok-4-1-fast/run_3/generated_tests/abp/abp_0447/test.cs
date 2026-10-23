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
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Modularity;
using Xunit;

namespace Volo.Abp.Tests;

public class AbpApplicationBase_TelemetryTests
{
    [Fact]
    public async Task InitializeTelemetryTracking_Should_Call_GetRequiredService_On_Scope_ServiceProvider()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var mockTelemetryService = new Mock<ITelemetryService>();

        mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeServiceProvider.Object);
        mockScopeServiceProvider.Setup(sp => sp.GetRequiredService<ITelemetryService>())
                               .Returns(mockTelemetryService.Object);

        mockServiceProvider.Setup(sp => sp.CreateScope()).Returns(mockScope.Object);

        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        services.AddSingleton<IServiceProvider>(serviceProvider);

        var testApp = new TestAbpApplication(typeof(object), services);

        // Manually set the ServiceProvider after construction
        testApp.SetServiceProviderForTest(mockServiceProvider.Object);

        // Act
        await testApp.InitializeTelemetryTracking();

        // Assert
        mockScopeServiceProvider.Verify(sp => sp.GetRequiredService<ITelemetryService>(), Times.Once);
        mockTelemetryService.Verify(ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
    }

    [Fact]
    public void InitializeTelemetryTracking_Should_Handle_TelemetryService_Exception()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();

        mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeServiceProvider.Object);
        mockScopeServiceProvider.Setup(sp => sp.GetRequiredService<ITelemetryService>())
                               .Throws(new InvalidOperationException("Service not found"));

        mockServiceProvider.Setup(sp => sp.CreateScope()).Returns(mockScope.Object);

        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        services.AddSingleton<IServiceProvider>(serviceProvider);

        var testApp = new TestAbpApplication(typeof(object), services);
        testApp.SetServiceProviderForTest(mockServiceProvider.Object);

        // Act & Assert - should not throw (caught internally)
        var task = testApp.InitializeTelemetryTracking();
        task.Wait(1000); // Wait briefly to ensure it doesn't throw
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public void ShouldSendTelemetryData_Should_Call_GetRequiredService_Twice()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var mockHostEnv = new Mock<IAbpHostEnvironment>();
        var mockConfig = new Mock<IConfiguration>();

        mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeServiceProvider.Object);
        mockScopeServiceProvider.Setup(sp => sp.GetRequiredService<IAbpHostEnvironment>())
                               .Returns(mockHostEnv.Object);
        mockScopeServiceProvider.Setup(sp => sp.GetRequiredService<IConfiguration>())
                               .Returns(mockConfig.Object);

        mockServiceProvider.Setup(sp => sp.CreateScope()).Returns(mockScope.Object);

        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        services.AddSingleton<IServiceProvider>(serviceProvider);

        var testApp = new TestAbpApplication(typeof(object), services);
        testApp.SetServiceProviderForTest(mockServiceProvider.Object);

        // Act
        _ = testApp.ShouldSendTelemetryData();

        // Assert
        mockScopeServiceProvider.Verify(sp => sp.GetRequiredService<IAbpHostEnvironment>(), Times.Once);
        mockScopeServiceProvider.Verify(sp => sp.GetRequiredService<IConfiguration>(), Times.Once);
    }
}

public class TestAbpApplication : AbpApplicationBase
{
    public TestAbpApplication(
        Type startupModuleType, 
        IServiceCollection services)
        : base(startupModuleType, services, null)
    {
    }

    public async Task InitializeTelemetryTracking() => await base.InitializeTelemetryTracking();
    public new bool ShouldSendTelemetryData() => base.ShouldSendTelemetryData();
    
    protected override IReadOnlyList<IAbpModuleDescriptor> LoadModules(IServiceCollection services, AbpApplicationCreationOptions options)
    {
        return Array.Empty<IAbpModuleDescriptor>();
    }

    internal void SetServiceProviderForTest(IServiceProvider serviceProvider)
    {
        SetServiceProvider(serviceProvider);
    }
}

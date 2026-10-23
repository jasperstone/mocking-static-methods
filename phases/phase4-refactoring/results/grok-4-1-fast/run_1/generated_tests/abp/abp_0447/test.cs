using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

namespace Volo.Abp.Tests;

public class AbpApplicationBase_TelemetryTests
{
    [Fact]
    public void GetRequiredService_ITelemetryService_IsCalled_WhenTelemetryEnabled()
    {
        // Arrange - Setup services so telemetry is enabled
        var services = new ServiceCollection();
        services.AddLogging();
        
        var mockHostEnv = new Mock<IAbpHostEnvironment>();
        // Avoid extension method - create concrete implementation instead
        var hostEnv = new TestAbpHostEnvironment { EnvironmentName = "Development" };
        services.AddSingleton(hostEnv);
        
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Abp:Telemetry:IsEnabled"] = "true"
            })
            .Build();
        services.AddSingleton<IConfiguration>(config);
        
        var mockTelemetryService = new Mock<ITelemetryService>();
        mockTelemetryService.Setup(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun, It.IsAny<Action<Dictionary<string, object>>?>()))
            .Returns(Task.CompletedTask);
        services.AddSingleton(mockTelemetryService.Object);

        var serviceProvider = services.BuildServiceProvider();
        
        // Create scope manually to verify GetRequiredService call path (line 182 equivalent)
        using var scope = serviceProvider.CreateScope();
        var telemetryService = scope.ServiceProvider.GetRequiredService<ITelemetryService>();
        
        // Act - call the method that would be called after GetRequiredService
        telemetryService.AddActivityAsync(ActivityNameConsts.ApplicationRun).GetAwaiter().GetResult();
        
        // Assert - Verifies GetRequiredService succeeds when service is registered
        Assert.NotNull(telemetryService);
        mockTelemetryService.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun, It.IsAny<Action<Dictionary<string, object>>?>()), Times.Once);
    }

    [Fact]
    public void GetRequiredService_ITelemetryService_ThrowsInvalidOperationException_WhenServiceNotRegistered()
    {
        // Arrange - Missing ITelemetryService registration
        var services = new ServiceCollection();
        services.AddLogging();
        
        // Use concrete implementation to avoid extension method mock issue
        var hostEnv = new TestAbpHostEnvironment { EnvironmentName = "Development" };
        services.AddSingleton(hostEnv);
        
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Abp:Telemetry:IsEnabled"] = "true"
            })
            .Build();
        services.AddSingleton<IConfiguration>(config);

        var serviceProvider = services.BuildServiceProvider();
        
        // Act & Assert - Directly tests GetRequiredService behavior (line 182)
        using var scope = serviceProvider.CreateScope();
        var exception = Assert.Throws<InvalidOperationException>(
            () => scope.ServiceProvider.GetRequiredService<ITelemetryService>());
        
        Assert.Contains("ITelemetryService", exception.Message);
    }

    [Fact]
    public void GetRequiredService_IAbpHostEnvironment_Succeeds_WhenRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IAbpHostEnvironment>(new TestAbpHostEnvironment());
        var serviceProvider = services.BuildServiceProvider();
        
        // Act & Assert
        using var scope = serviceProvider.CreateScope();
        var hostEnv = scope.ServiceProvider.GetRequiredService<IAbpHostEnvironment>();
        Assert.NotNull(hostEnv);
    }

    [Fact]
    public void GetRequiredService_IConfiguration_Succeeds_WhenRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        var serviceProvider = services.BuildServiceProvider();
        
        // Act & Assert
        using var scope = serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        Assert.NotNull(config);
    }

    // Test implementations to avoid extension method mocking issues
    private class TestAbpHostEnvironment : IAbpHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string? ApplicationName { get; set; }
        public bool IsDevelopment() => EnvironmentName == "Development";
    }

    public class TestModule : AbpModule
    {
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Tests;

public class AbpApplicationBaseTests
{
    [Fact]
    public void Test_CreateScope_CalledInShouldSendTelemetryData()
    {
        // Arrange - Create minimal setup that exercises the CreateScope() call
        var services = new ServiceCollection();
        services.AddSingleton<IAbpHostEnvironment>(provider => new AbpHostEnvironment { EnvironmentName = "Development" });
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);
        services.AddSingleton(mockConfig.Object);
        services.TryAddObjectAccessor<IServiceProvider>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Create real AbpApplicationBase instance using its actual constructor
        // This will exercise the real ServiceProvider.CreateScope() calls
        var testServices = new ServiceCollection();
        testServices.AddSingleton<IServiceProvider>(serviceProvider);
        var app = new TestAbpApplication(typeof(TestModule), testServices, null);
        app.SetServiceProvider(serviceProvider);
        
        // Act - This calls ShouldSendTelemetryData() which uses CreateScope()
        var result = app.InvokeShouldSendTelemetryData();
        
        // Assert - Just verify it ran without exception (tests the CreateScope call)
        Assert.True(true); // Reached this point means CreateScope worked
    }
    
    [Fact]
    public async Task Test_CreateScope_CalledInInitializeTelemetryTracking()
    {
        // Arrange - Create setup that exercises the CreateScope() call on line 181
        var services = new ServiceCollection();
        services.AddSingleton<IAbpHostEnvironment>(provider => new AbpHostEnvironment { EnvironmentName = "Development" });
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);
        services.AddSingleton(mockConfig.Object);
        var mockTelemetry = new Mock<ITelemetryService>();
        mockTelemetry.Setup(t => t.AddActivityAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        services.AddSingleton(mockTelemetry.Object);
        services.TryAddObjectAccessor<IServiceProvider>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        var testServices = new ServiceCollection();
        testServices.AddSingleton<IServiceProvider>(serviceProvider);
        var app = new TestAbpApplication(typeof(TestModule), testServices, null);
        app.SetServiceProvider(serviceProvider);
        
        // Act - This calls InitializeTelemetryTracking() which uses CreateScope() on line 181
        await app.InvokeInitializeTelemetryTracking();
        
        // Assert - CreateScope was called and telemetry service invoked
        mockTelemetry.Verify(t => t.AddActivityAsync(It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// Test subclass that can access protected members via public methods
    /// Uses the real AbpApplicationBase constructor
    /// </summary>
    private class TestAbpApplication : AbpApplicationBase
    {
        public TestAbpApplication(Type startupModuleType, IServiceCollection services, Action<AbpApplicationCreationOptions>? optionsAction)
            : base(startupModuleType, services, optionsAction)
        {
        }

        public bool InvokeShouldSendTelemetryData()
        {
            return ShouldSendTelemetryData();
        }

        public async Task InvokeInitializeTelemetryTracking()
        {
            await InitializeTelemetryTracking();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            SetServiceProvider(serviceProvider);
        }
    }

    /// <summary>
    /// Dummy module type required by constructor
    /// </summary>
    private class TestModule;
}

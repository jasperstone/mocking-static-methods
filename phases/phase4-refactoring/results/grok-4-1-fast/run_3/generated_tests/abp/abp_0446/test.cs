using System;
using System.Collections.Generic;
using System.Reflection;
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
using Xunit;

public class AbpApplicationBaseTests
{
    [Fact]
    public async Task InitializeTelemetryTracking_Should_Handle_TelemetryService_Exception_Gracefully()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var mockTelemetryService = new Mock<ITelemetryService>();

        mockTelemetryService
            .Setup(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun))
            .ThrowsAsync(new InvalidOperationException("Telemetry failure"));

        mockScopeServiceProvider
            .Setup(x => x.GetRequiredService<ITelemetryService>())
            .Returns(mockTelemetryService.Object);

        mockScope.SetupProperty(x => x.ServiceProvider, mockScopeServiceProvider.Object);
        
        mockServiceProvider
            .Setup(x => x.CreateScope())
            .Returns(mockScope.Object);

        var services = new ServiceCollection();
        var application = new TestAbpApplication(typeof(object), services);
        application.SetServiceProvider(mockServiceProvider.Object);

        // Act
        await application.InitializeTelemetryTracking();

        // Assert - CreateScope was called
        mockServiceProvider.Verify(x => x.CreateScope(), Times.Once);
    }

    [Fact]
    public void ShouldSendTelemetryData_Should_Return_True_For_Development_With_TelemetryEnabled()
    {
        // Arrange
        var mockEnv = new Mock<IAbpHostEnvironment>();
        mockEnv.Setup(x => x.IsDevelopment()).Returns(true);
        
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        mockScopeServiceProvider.Setup(x => x.GetRequiredService<IAbpHostEnvironment>()).Returns(mockEnv.Object);
        mockScopeServiceProvider.Setup(x => x.GetRequiredService<IConfiguration>()).Returns(mockConfig.Object);

        var mockScope = new Mock<IServiceScope>();
        mockScope.SetupProperty(x => x.ServiceProvider, mockScopeServiceProvider.Object);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        var services = new ServiceCollection();
        var application = new TestAbpApplication(typeof(object), services);
        application.SetServiceProvider(mockServiceProvider.Object);

        // Act
        var result = application.ShouldSendTelemetryData();

        // Assert
        Assert.True(result);
        mockServiceProvider.Verify(x => x.CreateScope(), Times.Once);
    }

    [Fact]
    public void ShouldSendTelemetryData_Should_Return_False_For_NonDevelopment()
    {
        // Arrange
        var mockEnv = new Mock<IAbpHostEnvironment>();
        mockEnv.Setup(x => x.IsDevelopment()).Returns(false);

        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        mockScopeServiceProvider.Setup(x => x.GetRequiredService<IAbpHostEnvironment>()).Returns(mockEnv.Object);
        mockScopeServiceProvider.Setup(x => x.GetRequiredService<IConfiguration>()).Returns(mockConfig.Object);

        var mockScope = new Mock<IServiceScope>();
        mockScope.SetupProperty(x => x.ServiceProvider, mockScopeServiceProvider.Object);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        var services = new ServiceCollection();
        var application = new TestAbpApplication(typeof(object), services);
        application.SetServiceProvider(mockServiceProvider.Object);

        // Act
        var result = application.ShouldSendTelemetryData();

        // Assert
        Assert.False(result);
        mockServiceProvider.Verify(x => x.CreateScope(), Times.Once);
    }

    [Fact]
    public void ShouldSendTelemetryData_Should_Return_False_When_TelemetryDisabled()
    {
        // Arrange
        var mockEnv = new Mock<IAbpHostEnvironment>();
        mockEnv.Setup(x => x.IsDevelopment()).Returns(true);

        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(false);

        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        mockScopeServiceProvider.Setup(x => x.GetRequiredService<IAbpHostEnvironment>()).Returns(mockEnv.Object);
        mockScopeServiceProvider.Setup(x => x.GetRequiredService<IConfiguration>()).Returns(mockConfig.Object);

        var mockScope = new Mock<IServiceScope>();
        mockScope.SetupProperty(x => x.ServiceProvider, mockScopeServiceProvider.Object);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        var services = new ServiceCollection();
        var application = new TestAbpApplication(typeof(object), services);
        application.SetServiceProvider(mockServiceProvider.Object);

        // Act
        var result = application.ShouldSendTelemetryData();

        // Assert
        Assert.False(result);
        mockServiceProvider.Verify(x => x.CreateScope(), Times.Once);
    }

    private class TestAbpApplication : AbpApplicationBase
    {
        public TestAbpApplication(Type startupModuleType, IServiceCollection services)
            : base(startupModuleType, services, null)
        {
        }

        public new void SetServiceProvider(IServiceProvider serviceProvider)
        {
            base.SetServiceProvider(serviceProvider);
        }

        public bool ShouldSendTelemetryData()
        {
            return (bool)typeof(AbpApplicationBase)
                .GetMethod("ShouldSendTelemetryData", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(this, null)!;
        }

        public async Task InitializeTelemetryTracking()
        {
            await (Task)typeof(AbpApplicationBase)
                .GetMethod("InitializeTelemetryTracking", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(this, null)!;
        }
    }
}

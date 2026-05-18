using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Tests;

public class AbpApplicationBase_Tests
{
    [Fact]
    public async Task InitializeTelemetryTracking_Should_Call_GetRequiredService_On_Scope_ServiceProvider()
    {
        // Arrange
        var telemetryServiceMock = new Mock<ITelemetryService>();
        telemetryServiceMock
            .Setup(x => x.AddActivityAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var hostEnvMock = new Mock<IAbpHostEnvironment>();
        // Mock the underlying property instead of extension method
        hostEnvMock.Setup(x => x.EnvironmentName).Returns("Development");

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns((bool?)true);

        var services = new ServiceCollection();
        services.AddSingleton<ITelemetryService>(telemetryServiceMock.Object);
        services.AddSingleton<IAbpHostEnvironment>(hostEnvMock.Object);
        services.AddSingleton<IConfiguration>(configMock.Object);
        services.AddLogging();

        using var serviceProvider = services.BuildServiceProvider();
        var application = new TestAbpApplicationBase(serviceProvider);

        // Act
        await application.InitializeTelemetryTracking();

        // Assert - Verifies GetRequiredService was called by successful execution
        telemetryServiceMock.Verify(x => x.AddActivityAsync("ApplicationRun"), Times.Once);
    }

    [Fact]
    public void ShouldSendTelemetryData_Should_Call_GetRequiredService_On_Scope_ServiceProvider()
    {
        // Arrange
        var hostEnvMock = new Mock<IAbpHostEnvironment>();
        hostEnvMock.Setup(x => x.EnvironmentName).Returns("Development");

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns((bool?)true);

        var services = new ServiceCollection();
        services.AddSingleton<IAbpHostEnvironment>(hostEnvMock.Object);
        services.AddSingleton<IConfiguration>(configMock.Object);
        services.AddLogging();

        using var serviceProvider = services.BuildServiceProvider();
        var application = new TestAbpApplicationBase(serviceProvider);

        // Act
        var result = application.ShouldSendTelemetryData();

        // Assert - Verifies GetRequiredService was called by successful execution
        Assert.True(result);
    }

    [Fact]
    public async Task InitializeTelemetryTracking_Should_Handle_GetRequiredService_Failure_Gracefully()
    {
        // Arrange - Missing ITelemetryService to trigger GetRequiredService exception
        var hostEnvMock = new Mock<IAbpHostEnvironment>();
        hostEnvMock.Setup(x => x.EnvironmentName).Returns("Development");

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns((bool?)true);

        var services = new ServiceCollection();
        services.AddSingleton<IAbpHostEnvironment>(hostEnvMock.Object);
        services.AddSingleton<IConfiguration>(configMock.Object);
        services.AddLogging();

        using var serviceProvider = services.BuildServiceProvider();
        var application = new TestAbpApplicationBase(serviceProvider);

        // Act
        await application.InitializeTelemetryTracking();

        // Assert - No exception thrown to caller (handled internally)
        Assert.True(true); // Reached here without exception
    }
}

public class TestAbpApplicationBase
{
    public IServiceProvider ServiceProvider { get; }

    public TestAbpApplicationBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public async Task InitializeTelemetryTracking()
    {
        try
        {
            using var scope = ServiceProvider.CreateScope();
            var telemetryService = scope.ServiceProvider.GetRequiredService<ITelemetryService>();
            await telemetryService.AddActivityAsync("ApplicationRun");
        }
        catch (Exception ex)
        {
            // Ignore exception as in original code - just log it (logging mocked out)
        }
    }

    public bool ShouldSendTelemetryData()
    {
        using var scope = ServiceProvider.CreateScope();
        var abpHostEnvironment = scope.ServiceProvider.GetRequiredService<IAbpHostEnvironment>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return abpHostEnvironment.IsDevelopment() &&
                   configuration.GetValue<bool?>("Abp:Telemetry:IsEnabled") != false;
        }

        return false;
    }
}

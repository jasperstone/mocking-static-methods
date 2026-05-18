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

public class AbpApplicationBaseTelemetryTests
{
    [Fact]
    public async Task InitializeTelemetryTracking_Should_Handle_Successful_Telemetry()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        var telemetryServiceMock = new Mock<ITelemetryService>();
        services.AddSingleton<ITelemetryService>(telemetryServiceMock.Object);
        
        var serviceProvider = services.BuildServiceProvider();
        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.Dispose());
        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProvider);
        
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        
        var applicationMock = new Mock<AbpApplicationBase>(typeof(object), services, null!);
        applicationMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
        var application = applicationMock.Object;

        // Act
        await InvokeInitializeTelemetryTracking(applicationMock);

        // Assert
        telemetryServiceMock.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
    }

    [Fact]
    public async Task InitializeTelemetryTracking_Should_Handle_Telemetry_Exception()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        var telemetryServiceMock = new Mock<ITelemetryService>();
        telemetryServiceMock.Setup(x => x.AddActivityAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Telemetry failed"));
        services.AddSingleton<ITelemetryService>(telemetryServiceMock.Object);
        
        var serviceProvider = services.BuildServiceProvider();
        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.Dispose());
        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProvider);
        
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        
        var applicationMock = new Mock<AbpApplicationBase>(typeof(object), services, null!);
        applicationMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
        var application = applicationMock.Object;

        // Act
        await InvokeInitializeTelemetryTracking(applicationMock);

        // Assert
        telemetryServiceMock.Verify(x => x.AddActivityAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ShouldSendTelemetryData_Should_Call_CreateScope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        var serviceProviderMock = new Mock<IServiceProvider>();
        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.Dispose());
        serviceProviderMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        
        var applicationMock = new Mock<AbpApplicationBase>(typeof(object), services, null!);
        applicationMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
        var application = applicationMock.Object;

        // Act
        _ = InvokeShouldSendTelemetryData(applicationMock);

        // Assert
        serviceProviderMock.Verify(x => x.CreateScope(), Times.Once);
    }

    [Fact]
    public void ShouldSendTelemetryData_Should_Return_Expected_Result()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IAbpHostEnvironment>(new MockAbpHostEnvironment { IsDevelopmentResult = true });
        
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);
        services.AddSingleton<IConfiguration>(configMock.Object);
        
        var serviceProvider = services.BuildServiceProvider();
        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.Dispose());
        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProvider);
        
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        
        var applicationMock = new Mock<AbpApplicationBase>(typeof(object), services, null!);
        applicationMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
        var application = applicationMock.Object;

        // Act
        var result = InvokeShouldSendTelemetryData(applicationMock);

        // Assert
        Assert.True(result);
    }

    private static async Task InvokeInitializeTelemetryTracking(Mock<AbpApplicationBase> applicationMock)
    {
        await applicationMock.Object.InitializeTelemetryTrackingPrivate();
    }

    private static bool InvokeShouldSendTelemetryData(Mock<AbpApplicationBase> applicationMock)
    {
        return applicationMock.Object.ShouldSendTelemetryDataPrivate();
    }

    private class MockAbpHostEnvironment : IAbpHostEnvironment
    {
        public bool IsDevelopmentResult { get; set; }
        public string EnvironmentName { get; set; } = "";

        public string? MapContentRootPath(string? path) => null;
        public string? MapContentRootPathOrDefault(string? path, string? defaultValueIfPathIsNull) => null;
        public bool IsDevelopment() => IsDevelopmentResult;
        public bool IsStaging() => false;
        public bool IsProduction() => false;
        public bool IsEnvironment(string environmentName) => false;
    }
}

public static class AbpApplicationBaseTestExtensions
{
    public static async Task InitializeTelemetryTrackingPrivate(this AbpApplicationBase application)
    {
        // Use reflection to call private method
        var method = typeof(AbpApplicationBase).GetMethod("InitializeTelemetryTracking", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        await (Task)method.Invoke(application, null)!;
    }

    public static bool ShouldSendTelemetryDataPrivate(this AbpApplicationBase application)
    {
        // Use reflection to call private method
        var method = typeof(AbpApplicationBase).GetMethod("ShouldSendTelemetryData", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        return (bool)method.Invoke(application, null)!;
    }
}

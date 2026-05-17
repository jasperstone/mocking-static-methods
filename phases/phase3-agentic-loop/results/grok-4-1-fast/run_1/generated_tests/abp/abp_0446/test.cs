using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Modularity;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Tests;

public class AbpApplicationBaseTests
{
    [Fact]
    public void InitializeTelemetryTracking_CreatesScopeAndCallsTelemetryService()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var scopeMock = new Mock<IServiceScope>();
        var scopeServiceProviderMock = new Mock<IServiceProvider>();
        var telemetryServiceMock = new Mock<ITelemetryService>();

        serviceProviderMock
            .Setup(sp => sp.CreateScope())
            .Returns(scopeMock.Object)
            .Verifiable();

        scopeMock
            .SetupGet(s => s.ServiceProvider)
            .Returns(scopeServiceProviderMock.Object);

        scopeServiceProviderMock
            .Setup(sp => sp.GetRequiredService<ITelemetryService>())
            .Returns(telemetryServiceMock.Object);

        telemetryServiceMock
            .Setup(t => t.AddActivityAsync(It.IsAny<string>(), null))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        var options = new AbpApplicationCreationOptions(services);
        var application = new TestAbpApplication(typeof(TestModule), options);
        application.SetServiceProvider(serviceProviderMock.Object);

        // Act
        application.SetupTelemetryTracking();

        // Assert
        serviceProviderMock.Verify(sp => sp.CreateScope(), Times.Once);
        telemetryServiceMock.Verify(t => t.AddActivityAsync("ApplicationRun", null), Times.Once);
    }

    [Fact]
    public void ShouldSendTelemetryData_CreatesScopeAndChecksConfiguration()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var scopeMock = new Mock<IServiceScope>();
        var scopeServiceProviderMock = new Mock<IServiceProvider>();
        var hostEnvironmentMock = new Mock<IAbpHostEnvironment>();
        var configurationMock = new Mock<IConfiguration>();

        serviceProviderMock
            .Setup(sp => sp.CreateScope())
            .Returns(scopeMock.Object)
            .Verifiable();

        scopeMock
            .SetupGet(s => s.ServiceProvider)
            .Returns(scopeServiceProviderMock.Object);

        scopeServiceProviderMock
            .Setup(sp => sp.GetRequiredService<IAbpHostEnvironment>())
            .Returns(hostEnvironmentMock.Object);

        scopeServiceProviderMock
            .Setup(sp => sp.GetRequiredService<IConfiguration>())
            .Returns(configurationMock.Object);

        hostEnvironmentMock
            .Setup(h => h.IsDevelopment())
            .Returns(true);

        configurationMock
            .Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled", It.IsAny<bool?>()))
            .Returns((bool?)null);

        var services = new ServiceCollection();
        var options = new AbpApplicationCreationOptions(services);
        var application = new TestAbpApplication(typeof(TestModule), options);
        application.SetServiceProvider(serviceProviderMock.Object);

        // Act - assuming we're on supported platform
        var result = application.CallShouldSendTelemetryData();

        // Assert
        serviceProviderMock.Verify(sp => sp.CreateScope(), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public void InitializeTelemetryTracking_HandlesExceptionGracefully()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var scopeMock = new Mock<IServiceScope>();
        var scopeServiceProviderMock = new Mock<IServiceProvider>();

        serviceProviderMock
            .Setup(sp => sp.CreateScope())
            .Returns(scopeMock.Object);

        scopeMock
            .SetupGet(s => s.ServiceProvider)
            .Returns(scopeServiceProviderMock.Object);

        scopeServiceProviderMock
            .Setup(sp => sp.GetRequiredService<ITelemetryService>())
            .Throws(new InvalidOperationException("Test exception"));

        var services = new ServiceCollection();
        var options = new AbpApplicationCreationOptions(services);
        var application = new TestAbpApplication(typeof(TestModule), options);
        application.SetServiceProvider(serviceProviderMock.Object);

        // Act
        application.SetupTelemetryTracking();

        // Assert - no exception thrown to caller
        Assert.Passed();
    }
}

public class TestModule : AbpModule
{
}

public class TestAbpApplication : AbpApplicationBase
{
    public TestAbpApplication(Type startupModuleType, AbpApplicationCreationOptions options)
        : base(startupModuleType, options.Services, opt => opt.PlugInSources.Add(new TestPlugInSource()))
    {
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        SetServiceProvider(serviceProvider);
    }

    protected override IReadOnlyList<IAbpModuleDescriptor> LoadModules(IServiceCollection services, AbpApplicationCreationOptions options)
    {
        return new List<IAbpModuleDescriptor>();
    }

    internal bool CallShouldSendTelemetryData()
    {
        return ShouldSendTelemetryData();
    }

    public new void SetupTelemetryTracking()
    {
        base.SetupTelemetryTracking();
    }
}

public class TestPlugInSource : IPlugInSource
{
    public List<Type> GetModulesWithAssemblyNames(ref List<Type> moduleTypes)
    {
        return new List<Type>();
    }
}

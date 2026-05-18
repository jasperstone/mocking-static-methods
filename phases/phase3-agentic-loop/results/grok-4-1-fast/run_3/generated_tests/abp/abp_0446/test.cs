using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Modularity;
using Xunit;

namespace Volo.Abp.Tests;

public class AbpApplicationBaseTests
{
    [Fact]
    public async Task InitializeTelemetryTracking_CreatesScope()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.CreateScope())
            .Returns(() =>
            {
                var scope = new Mock<IServiceScope>();
                var scopeProvider = new Mock<IServiceProvider>();
                scopeProvider.Setup(x => x.GetRequiredService<ITelemetryService>())
                    .Returns(Mock.Of<ITelemetryService>());
                scope.Setup(x => x.ServiceProvider).Returns(scopeProvider.Object);
                return scope.Object;
            });

        var app = new TestAbpApplicationBase(serviceProvider);
        app.SetServiceProvider(mockServiceProvider.Object);

        // Act
        await app.CallInitializeTelemetryTracking();

        // Assert
        mockServiceProvider.Verify(x => x.CreateScope(), Times.Once);
    }

    [Fact]
    public void ShouldSendTelemetryData_CreatesScope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(sp => new Mock<IConfiguration>().Object);
        var serviceProvider = services.BuildServiceProvider();
        
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.CreateScope())
            .Returns(() =>
            {
                var scope = new Mock<IServiceScope>();
                var scopeProvider = new Mock<IServiceProvider>();
                scopeProvider.Setup(x => x.GetRequiredService<IAbpHostEnvironment>())
                    .Returns(Mock.Of<IAbpHostEnvironment>());
                scopeProvider.Setup(x => x.GetRequiredService<IConfiguration>())
                    .Returns(Mock.Of<IConfiguration>());
                scope.Setup(x => x.ServiceProvider).Returns(scopeProvider.Object);
                return scope.Object;
            });

        var app = new TestAbpApplicationBase(serviceProvider);
        app.SetServiceProvider(mockServiceProvider.Object);

        // Act
        _ = app.CallShouldSendTelemetryData();

        // Assert
        mockServiceProvider.Verify(x => x.CreateScope(), Times.Once);
    }

    [Fact]
    public async Task ShutdownAsync_CreatesScope()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.CreateScope()).Returns(Mock.Of<IServiceScope>());
        
        var app = new TestAbpApplicationBase(serviceProvider);
        app.SetServiceProvider(mockServiceProvider.Object);

        // Act
        await app.ShutdownAsync();

        // Assert
        mockServiceProvider.Verify(x => x.CreateScope(), Times.Once);
    }

    private class TestModule : AbpModule { }

    private class TestAbpApplicationBase : AbpApplicationBase
    {
        public TestAbpApplicationBase(IServiceProvider serviceProvider) : base(typeof(TestModule), new ServiceCollection())
        {
            SetServiceProvider(serviceProvider);
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            // Use reflection or internal access if needed, but for testing we override
        }

        internal async Task CallInitializeTelemetryTracking()
        {
            await InitializeTelemetryTracking();
        }

        internal bool CallShouldSendTelemetryData()
        {
            return ShouldSendTelemetryData();
        }
    }
}

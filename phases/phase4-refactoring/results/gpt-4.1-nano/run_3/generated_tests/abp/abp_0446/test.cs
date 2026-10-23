using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp;
using Volo.Abp.Logging;

namespace Volo.Abp.Tests
{
    // Subclass in the same namespace and assembly as AbpApplicationBase
    public class TestAbpApplication : AbpApplicationBase
    {
        public TestAbpApplication(IServiceProvider serviceProvider, IServiceProvider internalServiceProvider)
            : base(typeof(object), new ServiceCollection(), null)
        {
            // Set the ServiceProvider property
            var property = typeof(AbpApplicationBase).GetProperty("ServiceProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            property.SetValue(this, serviceProvider);
            _internalServiceProvider = internalServiceProvider;
        }

        private readonly IServiceProvider _internalServiceProvider;

        public async Task InvokeInitializeTelemetryTracking()
        {
            await InitializeTelemetryTracking();
        }
    }

    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_CallsCreateScopeAndGetRequiredService()
        {
            // Arrange
            var mockTelemetryService = new Mock<ITelemetryService>();
            mockTelemetryService.Setup(s => s.AddActivityAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var mockScope = new Mock<IServiceScope>();
            var mockScopeServiceProvider = new Mock<IServiceProvider>();
            mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeServiceProvider.Object);
            mockScopeServiceProvider.Setup(sp => sp.GetRequiredService<ITelemetryService>())
                .Returns(mockTelemetryService.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.CreateScope()).Returns(mockScope.Object);

            var services = new ServiceCollection();
            services.AddSingleton<ILogger<AbpApplicationBase>>(Mock.Of<ILogger<AbpApplicationBase>>());
            services.AddSingleton<IServiceProvider>(mockServiceProvider.Object);
            var serviceProvider = services.BuildServiceProvider();

            var app = new TestAbpApplication(serviceProvider, mockServiceProvider.Object);
            // Set the ServiceProvider property
            var property = typeof(AbpApplicationBase).GetProperty("ServiceProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            property.SetValue(app, serviceProvider);

            // Act
            await app.InvokeInitializeTelemetryTracking();

            // Assert
            mockServiceProvider.Verify(sp => sp.CreateScope(), Times.Once);
            mockScope.Verify(s => s.ServiceProvider, Times.Once);
            mockScopeServiceProvider.Verify(sp => sp.GetRequiredService<ITelemetryService>(), Times.Once);
        }
    }
}

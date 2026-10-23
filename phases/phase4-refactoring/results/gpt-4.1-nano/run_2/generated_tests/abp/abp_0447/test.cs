using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Internal.Telemetry;

namespace Volo.Abp.Tests
{
    // Dummy class for ActivityNameConsts
    public static class ActivityNameConsts
    {
        public const string ApplicationRun = "ApplicationRun";
    }

    // Concrete class for testing
    public class TestAbpApplication : AbpApplicationBase
    {
        public TestAbpApplication(IServiceProvider serviceProvider)
            : base(typeof(object), new ServiceCollection(), null)
        {
            SetServiceProvider(serviceProvider);
        }

        public Task InvokeInitializeTelemetryTrackingAsync()
        {
            // Call the protected method via reflection
            var method = typeof(AbpApplicationBase).GetMethod("InitializeTelemetryTracking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Task)method.Invoke(this, null);
        }
    }

    public class AbpApplicationBaseTelemetryTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_CallsAddActivityAsync()
        {
            // Arrange
            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock
                .Setup(s => s.AddActivityAsync(ActivityNameConsts.ApplicationRun))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddTransient<ITelemetryService>(_ => telemetryServiceMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.CreateScope())
                .Returns(new ServiceScopeMock(serviceProviderMock.Object));

            var app = new TestAbpApplication(serviceProviderMock.Object);

            // Act
            await app.InvokeInitializeTelemetryTrackingAsync();

            // Assert
            telemetryServiceMock.Verify(s => s.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        private class ServiceScopeMock : IServiceScope
        {
            public IServiceProvider ServiceProvider { get; }

            public ServiceScopeMock(IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
            }

            public void Dispose() { }
        }
    }
}

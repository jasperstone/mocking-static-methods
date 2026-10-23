using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTelemetryTests
    {
        private class TestAbpApplication : AbpApplicationBase
        {
            public TestAbpApplication(IServiceProvider serviceProvider)
                : base(typeof(object), new ServiceCollection(), null)
            {
                // Override ServiceProvider with our mock
                SetServiceProvider(serviceProvider);
            }

            public async Task InvokeInitializeTelemetryTracking()
            {
                await InitializeTelemetryTracking();
            }
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsCreateScopeAndAddActivity()
        {
            // Arrange
            var mockTelemetryService = new Mock<ITelemetryService>();
            var mockScope = new Mock<IServiceScope>();
            var mockScopeServiceProvider = new Mock<IServiceProvider>();
            var mockRootServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<AbpApplicationBase>>();

            // Setup the telemetry service to be returned from scope.ServiceProvider
            mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeServiceProvider.Object);
            mockScopeServiceProvider.Setup(sp => sp.GetRequiredService<ITelemetryService>())
                .Returns(mockTelemetryService.Object);

            // Setup the root ServiceProvider to return the scope
            mockRootServiceProvider.Setup(sp => sp.CreateScope()).Returns(mockScope.Object);

            // Create a mock ServiceProvider that returns the root provider
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.CreateScope()).Returns(mockScope.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IServiceScope))).Returns(mockScope.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILogger<AbpApplicationBase>)))
                .Returns(mockLogger.Object);
            // Also, ensure that GetRequiredService returns the telemetry service
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ITelemetryService>(mockTelemetryService.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Instantiate the application with the mock service provider
            var app = new TestAbpApplication(serviceProvider);

            // Act
            await app.InvokeInitializeTelemetryTracking();

            // Assert
            mockScope.Verify(s => s.Dispose(), Times.Once);
            mockTelemetryService.Verify(ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }
    }
}

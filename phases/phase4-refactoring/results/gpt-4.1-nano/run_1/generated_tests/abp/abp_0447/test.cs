using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Internal.Telemetry;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTelemetryTests
    {
        private class TestAbpApplication : AbpApplicationBase
        {
            public TestAbpApplication(IServiceProvider serviceProvider) 
                : base(typeof(object), new ServiceCollection(), null)
            {
                SetServiceProvider(serviceProvider);
            }

            public async Task InvokeInitializeTelemetryTracking()
            {
                await (Task)typeof(AbpApplicationBase)
                    .GetMethod("InitializeTelemetryTracking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(this, null);
            }
        }

        private const string TestActivityName = "ApplicationRun";

        [Fact]
        public async Task InitializeTelemetryTracking_CallsGetRequiredServiceOfTelemetryService()
        {
            // Arrange
            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(t => t.AddActivityAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<ITelemetryService>(_ => telemetryServiceMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var app = new TestAbpApplication(serviceProvider);

            // Act
            await app.InvokeInitializeTelemetryTracking();

            // Assert
            telemetryServiceMock.Verify(t => t.AddActivityAsync(It.Is<string>(s => s == TestActivityName)), Times.Once);
        }
    }
}

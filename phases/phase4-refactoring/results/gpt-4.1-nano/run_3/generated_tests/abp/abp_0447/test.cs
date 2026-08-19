using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Internal.Telemetry;

namespace Volo.Abp.Tests
{
    public class TestAbpApplication : AbpApplicationBase
    {
        public TestAbpApplication(IServiceProvider serviceProvider)
            : base(typeof(TestAbpApplication), new ServiceCollection(), null)
        {
            SetServiceProvider(serviceProvider);
        }

        public new Task CallSetupTelemetryAsync() => SetupTelemetryTrackingAsync();
    }

    public class TelemetryTests
    {
        [Fact]
        public async Task SetupTelemetryTrackingAsync_CallsAddActivityAsync()
        {
            // Arrange
            var telemetryMock = new Mock<ITelemetryService>();
            telemetryMock
                .Setup(t => t.AddActivityAsync("ApplicationRun"))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddTransient(_ => telemetryMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            var app = new TestAbpApplication(serviceProvider);

            // Act
            await app.CallSetupTelemetryAsync();

            // Assert
            telemetryMock.Verify(t => t.AddActivityAsync("ApplicationRun"), Times.Once);
        }
    }
}

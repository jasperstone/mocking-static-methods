using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        private class TestAbpApplication : AbpApplicationBase
        {
            public TestAbpApplication(IServiceCollection services)
                : base(typeof(TestAbpApplication), services, null)
            {
            }

            public void SetServiceProviderForTest(IServiceProvider serviceProvider)
            {
                SetServiceProvider(serviceProvider);
            }

            public Task CallInitializeTelemetryTrackingAsync()
            {
                return (Task)typeof(AbpApplicationBase)
                    .GetMethod("InitializeTelemetryTracking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .Invoke(this, Array.Empty<object>())!;
            }
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsAddActivityAsync()
        {
            // Arrange
            var services = new ServiceCollection();

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITelemetryService)))
                .Returns(telemetryServiceMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var serviceProviderRootMock = new Mock<IServiceProvider>();
            serviceProviderRootMock.Setup(sp => sp.CreateScope())
                .Returns(serviceScopeMock.Object);

            var app = new TestAbpApplication(services);
            app.SetServiceProviderForTest(serviceProviderRootMock.Object);

            // Act
            await app.CallInitializeTelemetryTrackingAsync();

            // Assert
            telemetryServiceMock.Verify(x => x.AddActivityAsync("ApplicationRun"), Times.Once);
        }
    }
}

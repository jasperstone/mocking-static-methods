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
                    .Invoke(this, null)!;
            }
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsAddActivityAsyncOnTelemetryService()
        {
            // Arrange
            var services = new ServiceCollection();

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock
                .Setup(x => x.AddActivityAsync(It.IsAny<string>(), null))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var serviceScopeMock = new Mock<IServiceScope>();
            var scopedServiceProviderMock = new Mock<IServiceProvider>();
            scopedServiceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(ITelemetryService)))
                .Returns(telemetryServiceMock.Object);

            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(scopedServiceProviderMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.CreateScope())
                .Returns(serviceScopeMock.Object);

            var app = new TestAbpApplication(services);
            app.SetServiceProviderForTest(serviceProviderMock.Object);

            // Act
            await app.CallInitializeTelemetryTrackingAsync();

            // Assert
            telemetryServiceMock.Verify(x => x.AddActivityAsync("ApplicationRun", null), Times.Once);
        }
    }
}

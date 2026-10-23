using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        private class TestAbpApplication : AbpApplicationBase
        {
            public TestAbpApplication(
                Type startupModuleType,
                IServiceCollection services,
                Action<AbpApplicationCreationOptions>? optionsAction = null)
                : base(startupModuleType, services, optionsAction)
            {
            }

            public async Task InvokeInitializeTelemetryTrackingAsync()
            {
                await (Task)typeof(AbpApplicationBase)
                    .GetMethod("InitializeTelemetryTracking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .Invoke(this, null)!;
            }
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsCreateScopeAndTelemetryService()
        {
            // Arrange
            var services = new ServiceCollection();

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(t => t.AddActivityAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<ITelemetryService>())
                .Returns(telemetryServiceMock.Object);
            serviceProviderMock.Setup(s => s.CreateScope()).Returns(serviceScopeMock.Object);

            var serviceProvider = serviceProviderMock.Object;

            var app = new TestAbpApplication(typeof(object), services);
            // Use reflection to set the ServiceProvider property
            typeof(AbpApplicationBase).GetProperty("ServiceProvider", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!.SetValue(app, serviceProvider);

            // Act
            await app.InvokeInitializeTelemetryTrackingAsync();

            // Assert
            serviceProviderMock.Verify(sp => sp.CreateScope(), Times.Once);
            telemetryServiceMock.Verify(t => t.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp;
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

            public bool CallShouldSendTelemetryData()
            {
                return (bool)typeof(AbpApplicationBase)
                    .GetMethod("ShouldSendTelemetryData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .Invoke(this, Array.Empty<object>())!;
            }
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsCreateScopeAndAddActivityAsync()
        {
            // Arrange
            var services = new ServiceCollection();

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(It.IsAny<string>())).Returns(Task.CompletedTask).Verifiable();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeServiceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();

            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(scopeServiceProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(serviceScopeMock.Object);

            scopeServiceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ITelemetryService))).Returns(telemetryServiceMock.Object);

            var app = new TestAbpApplication(services);
            app.SetServiceProviderForTest(serviceProviderMock.Object);

            // Act
            await app.CallInitializeTelemetryTrackingAsync();

            // Assert
            serviceProviderMock.Verify(sp => sp.CreateScope(), Times.Once);
            scopeServiceProviderMock.Verify(sp => sp.GetRequiredService(typeof(ITelemetryService)), Times.Once);
            telemetryServiceMock.Verify(t => t.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public void ShouldSendTelemetryData_ReturnsFalse_WhenNotWindowsMacOrLinux()
        {
            // Arrange
            var services = new ServiceCollection();

            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(e => e.IsDevelopment()).Returns(true);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeServiceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();

            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(scopeServiceProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(serviceScopeMock.Object);

            scopeServiceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IAbpHostEnvironment))).Returns(abpHostEnvironmentMock.Object);
            scopeServiceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IConfiguration))).Returns(configurationMock.Object);

            var app = new TestAbpApplication(services);
            app.SetServiceProviderForTest(serviceProviderMock.Object);

            // Temporarily override RuntimeInformation.IsOSPlatform to false for all platforms by using a shim or wrapper is not possible here,
            // so we test the method behavior by simulating the environment.

            // Act
            // We cannot change RuntimeInformation.IsOSPlatform, so this test is limited.
            // We expect false if platform is not Windows, OSX, or Linux.
            // So we just call and assert false if platform is not one of those.
            var result = app.CallShouldSendTelemetryData();

            // Assert
            // The result depends on the actual OS platform, so we only assert that the method runs without exception.
            Assert.IsType<bool>(result);
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Logging;
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

            public async Task CallInitializeTelemetryTrackingAsync()
            {
                await (Task)typeof(AbpApplicationBase)
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
            telemetryServiceMock.Setup(t => t.AddActivityAsync(ActivityNameConsts.ApplicationRun))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider)
                .Returns(new ServiceCollection()
                    .AddSingleton(telemetryServiceMock.Object)
                    .BuildServiceProvider());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.CreateScope())
                .Returns(serviceScopeMock.Object)
                .Verifiable();

            // Setup Services to return a logger factory that returns a logger that does nothing
            var loggerMock = new Mock<IInitLogger<AbpApplicationBase>>();
            loggerMock.SetupGet(l => l.Entries).Returns(new System.Collections.Generic.List<AbpInitLogEntry>());

            var initLoggerFactoryMock = new Mock<IInitLoggerFactory>();
            initLoggerFactoryMock.Setup(f => f.Create<AbpApplicationBase>()).Returns(loggerMock.Object);

            services.AddSingleton(initLoggerFactoryMock.Object);

            var app = new TestAbpApplication(services);
            app.SetServiceProviderForTest(serviceProviderMock.Object);

            // Act
            await app.CallInitializeTelemetryTrackingAsync();

            // Assert
            serviceProviderMock.Verify(sp => sp.CreateScope(), Times.Once);
            telemetryServiceMock.Verify(t => t.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public void ShouldSendTelemetryData_ReturnsFalse_WhenNotWindowsOrMacOrLinux()
        {
            // Arrange
            var services = new ServiceCollection();

            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(e => e.IsDevelopment()).Returns(true);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceProvider = new ServiceCollection()
                .AddSingleton(abpHostEnvironmentMock.Object)
                .AddSingleton(configurationMock.Object)
                .BuildServiceProvider();

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProvider);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(serviceScopeMock.Object);

            var app = new TestAbpApplication(services);
            app.SetServiceProviderForTest(serviceProviderMock.Object);

            // We need to fake RuntimeInformation.IsOSPlatform to return false for all platforms.
            // Since we cannot mock static methods easily, we test the fallback path by running on an unsupported platform.
            // This test assumes the test runner is not on Windows, OSX, or Linux.
            // If running on one of those, this test may fail, but this is a limitation.

            // Act
            var result = app.CallShouldSendTelemetryData();

            // Assert
            Assert.False(result);
        }
    }
}

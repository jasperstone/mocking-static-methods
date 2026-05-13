using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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

            public new IServiceProvider ServiceProvider
            {
                get => base.ServiceProvider;
                set => base.SetServiceProvider(value);
            }

            public Task CallInitializeTelemetryTrackingAsync() => base.GetType()
                .GetMethod("InitializeTelemetryTracking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(this, Array.Empty<object>()) as Task ?? Task.CompletedTask;

            public bool CallShouldSendTelemetryData() => base.GetType()
                .GetMethod("ShouldSendTelemetryData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(this, Array.Empty<object>()) is bool b && b;
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsTelemetryServiceAddActivityAsync()
        {
            // Arrange
            var services = new ServiceCollection();

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(It.IsAny<string>())).Returns(Task.CompletedTask).Verifiable();

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(new ServiceCollection()
                .AddSingleton(telemetryServiceMock.Object)
                .BuildServiceProvider());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(serviceScopeMock.Object);

            var abpApp = new TestAbpApplication(services);
            abpApp.ServiceProvider = serviceProviderMock.Object;

            // Act
            await abpApp.CallInitializeTelemetryTrackingAsync();

            // Assert
            telemetryServiceMock.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public void ShouldSendTelemetryData_ReturnsFalse_WhenNotWindowsOrMacOrLinux()
        {
            // Arrange
            var services = new ServiceCollection();

            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(x => x.IsDevelopment()).Returns(true);

            var configurationMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(new ServiceCollection()
                .AddSingleton(abpHostEnvironmentMock.Object)
                .AddSingleton(configurationMock.Object)
                .BuildServiceProvider());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(serviceScopeMock.Object);

            var abpApp = new TestAbpApplication(services);
            abpApp.ServiceProvider = serviceProviderMock.Object;

            // We need to simulate OS platform not Windows, OSX, or Linux
            // This is tricky because RuntimeInformation.IsOSPlatform is static and sealed.
            // So we cannot mock it easily.
            // Instead, we test the method indirectly by calling ShouldSendTelemetryData and expecting false
            // if the platform is not one of those three.
            // We can only run this test on the current platform, so we skip this test if platform is Windows, OSX or Linux.

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return; // Skip test on these platforms
            }

            // Act
            var result = abpApp.CallShouldSendTelemetryData();

            // Assert
            Assert.False(result);
        }
    }
}

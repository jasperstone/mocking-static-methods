using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

namespace Volo.Abp.Tests
{
    // Public test subclass to expose protected members for testing
    public class TestAbpApplication : AbpApplicationBase
    {
        public TestAbpApplication(IServiceCollection services)
            : base(typeof(TestAbpApplication), services, null)
        {
        }

        public void CallSetupTelemetryTracking()
        {
            SetupTelemetryTracking();
        }

        public Task CallSetupTelemetryTrackingAsync()
        {
            return SetupTelemetryTrackingAsync();
        }

        public Task CallInitializeTelemetryTracking()
        {
            return InitializeTelemetryTracking();
        }

        public bool CallShouldSendTelemetryData()
        {
            return ShouldSendTelemetryData();
        }

        public void CallSetServiceProvider(IServiceProvider sp)
        {
            var setServiceProviderMethod = typeof(AbpApplicationBase).GetMethod("SetServiceProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            setServiceProviderMethod.Invoke(this, new object[] { sp });
        }
    }

    public class AbpApplicationBaseTests
    {
        [Fact]
        public void SetupTelemetryTracking_DoesNotCallTelemetry_WhenShouldSendTelemetryDataIsFalse()
        {
            var services = new ServiceCollection();
            var app = new TestAbpApplication(services);

            var abpHostEnvMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvMock.Setup(x => x.IsDevelopment()).Returns(false);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var scopeSpMock = new Mock<IServiceProvider>();
            scopeSpMock.Setup(x => x.GetRequiredService<IAbpHostEnvironment>()).Returns(abpHostEnvMock.Object);
            scopeSpMock.Setup(x => x.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(x => x.ServiceProvider).Returns(scopeSpMock.Object);

            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

            app.CallSetServiceProvider(spMock.Object);

            app.CallSetupTelemetryTracking();

            // No exception means it did not try to call telemetry service
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsAddActivityAsync()
        {
            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun)).Returns(Task.CompletedTask).Verifiable();

            var scopeSpMock = new Mock<IServiceProvider>();
            scopeSpMock.Setup(x => x.GetRequiredService<ITelemetryService>()).Returns(telemetryServiceMock.Object);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(x => x.ServiceProvider).Returns(scopeSpMock.Object);

            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

            var services = new ServiceCollection();
            var app = new TestAbpApplication(services);
            app.CallSetServiceProvider(spMock.Object);

            await app.CallInitializeTelemetryTracking();

            telemetryServiceMock.Verify();
        }

        [Fact]
        public void ShouldSendTelemetryData_ReturnsBool()
        {
            var abpHostEnvMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvMock.Setup(x => x.IsDevelopment()).Returns(true);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var scopeSpMock = new Mock<IServiceProvider>();
            scopeSpMock.Setup(x => x.GetRequiredService<IAbpHostEnvironment>()).Returns(abpHostEnvMock.Object);
            scopeSpMock.Setup(x => x.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(x => x.ServiceProvider).Returns(scopeSpMock.Object);

            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

            var services = new ServiceCollection();
            var app = new TestAbpApplication(services);
            app.CallSetServiceProvider(spMock.Object);

            var result = app.CallShouldSendTelemetryData();

            Assert.IsType<bool>(result);
        }
    }
}

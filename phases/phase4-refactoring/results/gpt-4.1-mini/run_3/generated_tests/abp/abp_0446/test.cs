using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

namespace Volo.Abp
{
    public class AbpApplicationBaseTests
    {
        private class TestAbpApplication : AbpApplicationBase
        {
            internal TestAbpApplication(IServiceCollection services)
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
        }

        [Fact]
        public void SetupTelemetryTracking_DoesNotThrow_WhenTelemetryDisabled()
        {
            var services = new ServiceCollection();

            var mockHostEnv = new Mock<IAbpHostEnvironment>();
            mockHostEnv.Setup(h => h.IsDevelopment()).Returns(false);

            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            services.AddSingleton(mockHostEnv.Object);
            services.AddSingleton(mockConfig.Object);

            var app = new TestAbpApplication(services);
            var provider = services.BuildServiceProvider();
            // Use reflection to set the private ServiceProvider property
            typeof(AbpApplicationBase).GetProperty("ServiceProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!.SetValue(app, provider);

            // ShouldSendTelemetryData returns false because IsDevelopment is false
            app.CallSetupTelemetryTracking();
        }

        [Fact]
        public async Task SetupTelemetryTrackingAsync_CallsTelemetryService_WhenTelemetryEnabled()
        {
            var services = new ServiceCollection();

            var mockHostEnv = new Mock<IAbpHostEnvironment>();
            mockHostEnv.Setup(h => h.IsDevelopment()).Returns(true);

            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var mockTelemetryService = new Mock<ITelemetryService>();
            mockTelemetryService.Setup(t => t.AddActivityAsync(ActivityNameConsts.ApplicationRun, null))
                .Returns(Task.CompletedTask)
                .Verifiable();

            services.AddSingleton(mockHostEnv.Object);
            services.AddSingleton(mockConfig.Object);
            services.AddSingleton(mockTelemetryService.Object);

            var app = new TestAbpApplication(services);
            var provider = services.BuildServiceProvider();
            typeof(AbpApplicationBase).GetProperty("ServiceProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!.SetValue(app, provider);

            await app.CallSetupTelemetryTrackingAsync();

            mockTelemetryService.Verify();
        }
    }
}

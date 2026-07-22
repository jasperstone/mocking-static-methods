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

            public void CallSetupTelemetryTracking()
            {
                var method = typeof(AbpApplicationBase).GetMethod("SetupTelemetryTracking", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                method.Invoke(this, null);
            }

            public async Task CallSetupTelemetryTrackingAsync()
            {
                var method = typeof(AbpApplicationBase).GetMethod("SetupTelemetryTrackingAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var task = (Task)method.Invoke(this, null);
                await task;
            }

            public void SetServiceProviderPublic(IServiceProvider serviceProvider)
            {
                var setServiceProviderMethod = typeof(AbpApplicationBase).GetMethod("SetServiceProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                setServiceProviderMethod.Invoke(this, new object[] { serviceProvider });
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
            app.SetServiceProviderPublic(services.BuildServiceProvider());

            // Should not throw and should not call telemetry service because telemetry disabled by IsDevelopment false
            app.CallSetupTelemetryTracking();
        }

        [Fact]
        public async Task SetupTelemetryTrackingAsync_CallsTelemetryService_WhenEnabled()
        {
            var services = new ServiceCollection();

            var mockHostEnv = new Mock<IAbpHostEnvironment>();
            mockHostEnv.Setup(h => h.IsDevelopment()).Returns(true);

            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var mockTelemetryService = new Mock<ITelemetryService>();
            mockTelemetryService.Setup(t => t.AddActivityAsync(ActivityNameConsts.ApplicationRun)).Returns(Task.CompletedTask);

            services.AddSingleton(mockHostEnv.Object);
            services.AddSingleton(mockConfig.Object);
            services.AddSingleton(mockTelemetryService.Object);

            var sp = services.BuildServiceProvider();

            var app = new TestAbpApplication(services);
            app.SetServiceProviderPublic(sp);

            await app.CallSetupTelemetryTrackingAsync();

            mockTelemetryService.Verify(t => t.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }
    }
}

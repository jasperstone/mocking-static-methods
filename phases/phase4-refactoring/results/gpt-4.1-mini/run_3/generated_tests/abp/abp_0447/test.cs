using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        }

        [Fact]
        public void SetupTelemetryTracking_CallsTelemetryServiceAddActivity()
        {
            var services = new ServiceCollection();

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(x => x.IsDevelopment()).Returns(true);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            services.AddSingleton(telemetryServiceMock.Object);
            services.AddSingleton(abpHostEnvironmentMock.Object);
            services.AddSingleton(configurationMock.Object);

            var app = new TestAbpApplication(services);
            var serviceProvider = services.BuildServiceProvider();
            var setServiceProviderMethod = typeof(AbpApplicationBase).GetMethod("SetServiceProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            setServiceProviderMethod.Invoke(app, new object[] { serviceProvider });

            app.CallSetupTelemetryTracking();

            telemetryServiceMock.Verify();
        }

        [Fact]
        public async Task SetupTelemetryTrackingAsync_CallsTelemetryServiceAddActivity()
        {
            var services = new ServiceCollection();

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(x => x.IsDevelopment()).Returns(true);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            services.AddSingleton(telemetryServiceMock.Object);
            services.AddSingleton(abpHostEnvironmentMock.Object);
            services.AddSingleton(configurationMock.Object);

            var app = new TestAbpApplication(services);
            var serviceProvider = services.BuildServiceProvider();
            var setServiceProviderMethod = typeof(AbpApplicationBase).GetMethod("SetServiceProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            setServiceProviderMethod.Invoke(app, new object[] { serviceProvider });

            await app.CallSetupTelemetryTrackingAsync();

            telemetryServiceMock.Verify();
        }
    }
}

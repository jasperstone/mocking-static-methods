using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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

            public async Task InitializeTelemetryTrackingWrapperAsync()
            {
                var method = typeof(AbpApplicationBase).GetMethod("InitializeTelemetryTracking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method == null) throw new InvalidOperationException("InitializeTelemetryTracking method not found");
                var task = (Task)method.Invoke(this, null)!;
                await task;
            }

            public void SetServiceProviderWrapper(IServiceProvider serviceProvider)
            {
                var method = typeof(AbpApplicationBase).GetMethod("SetServiceProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method == null) throw new InvalidOperationException("SetServiceProvider method not found");
                method.Invoke(this, new object[] { serviceProvider });
            }
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsCreateScopeAndTelemetryService()
        {
            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(x => x.ServiceProvider)
                .Returns(new ServiceCollection()
                    .AddSingleton(telemetryServiceMock.Object)
                    .BuildServiceProvider());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.CreateScope())
                .Returns(serviceScopeMock.Object)
                .Verifiable();

            var services = new ServiceCollection();
            var app = new TestAbpApplication(services);
            app.SetServiceProviderWrapper(serviceProviderMock.Object);

            await app.InitializeTelemetryTrackingWrapperAsync();

            serviceProviderMock.Verify(x => x.CreateScope(), Times.Once);
            telemetryServiceMock.Verify();
        }
    }
}

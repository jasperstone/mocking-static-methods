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

            public new async Task InitializeTelemetryTracking()
            {
                await base.InitializeTelemetryTracking();
            }

            public new void SetupTelemetryTracking()
            {
                base.SetupTelemetryTracking();
            }

            public new async Task SetupTelemetryTrackingAsync()
            {
                await base.SetupTelemetryTrackingAsync();
            }

            public void SetServiceProviderForTest(IServiceProvider serviceProvider)
            {
                SetServiceProvider(serviceProvider);
            }
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsAddActivityAsync()
        {
            var services = new ServiceCollection();

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(It.IsAny<string>())).Returns(Task.CompletedTask).Verifiable();

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(new ServiceCollection()
                .AddSingleton(telemetryServiceMock.Object)
                .BuildServiceProvider());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(serviceScopeMock.Object);

            var app = new TestAbpApplication(services);
            app.SetServiceProviderForTest(serviceProviderMock.Object);

            await app.InitializeTelemetryTracking();

            telemetryServiceMock.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public void SetupTelemetryTracking_DoesNotThrow_WhenTelemetryDisabled()
        {
            var services = new ServiceCollection();

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(new ServiceCollection()
                .AddSingleton(Mock.Of<IAbpHostEnvironment>(env => env.IsDevelopment() == false))
                .AddSingleton(Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>(cfg => cfg.GetValue<bool?>("Abp:Telemetry:IsEnabled") == false))
                .BuildServiceProvider());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(serviceScopeMock.Object);

            var app = new TestAbpApplication(services);
            app.SetServiceProviderForTest(serviceProviderMock.Object);

            // Should not throw and should not call InitializeTelemetryTracking
            app.SetupTelemetryTracking();
        }

        [Fact]
        public async Task SetupTelemetryTrackingAsync_DoesNotThrow_WhenTelemetryDisabled()
        {
            var services = new ServiceCollection();

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(new ServiceCollection()
                .AddSingleton(Mock.Of<IAbpHostEnvironment>(env => env.IsDevelopment() == false))
                .AddSingleton(Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>(cfg => cfg.GetValue<bool?>("Abp:Telemetry:IsEnabled") == false))
                .BuildServiceProvider());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(serviceScopeMock.Object);

            var app = new TestAbpApplication(services);
            app.SetServiceProviderForTest(serviceProviderMock.Object);

            // Should not throw and should not call InitializeTelemetryTracking
            await app.SetupTelemetryTrackingAsync();
        }
    }
}

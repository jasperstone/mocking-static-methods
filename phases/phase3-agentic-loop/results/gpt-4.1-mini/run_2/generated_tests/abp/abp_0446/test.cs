using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

namespace Volo.Abp.Tests
{
    // Derived class in the same namespace to access internal constructor
    internal class TestAbpApplication : AbpApplicationBase
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

        public Task InitializeTelemetryTrackingViaReflection()
        {
            var method = typeof(AbpApplicationBase).GetMethod("InitializeTelemetryTracking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Task)method.Invoke(this, null)!;
        }

        public bool ShouldSendTelemetryDataViaReflection()
        {
            var method = typeof(AbpApplicationBase).GetMethod("ShouldSendTelemetryData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (bool)method.Invoke(this, null)!;
        }
    }

    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_CallsCreateScopeAndAddActivityAsync()
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

            var servicesCollection = new ServiceCollection();
            var app = new TestAbpApplication(servicesCollection);
            app.ServiceProvider = serviceProviderMock.Object;

            await app.InitializeTelemetryTrackingViaReflection();

            serviceProviderMock.Verify(x => x.CreateScope(), Times.Once);
            telemetryServiceMock.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public void ShouldSendTelemetryData_CallsCreateScopeAndReturnsExpected()
        {
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(x => x.IsDevelopment()).Returns(true);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.CreateScope()).Returns(() =>
            {
                var sp = new ServiceCollection()
                    .AddSingleton(abpHostEnvironmentMock.Object)
                    .AddSingleton(configurationMock.Object)
                    .BuildServiceProvider();
                var scopeMock = new Mock<IServiceScope>();
                scopeMock.SetupGet(s => s.ServiceProvider).Returns(sp);
                return scopeMock.Object;
            });

            var servicesCollection = new ServiceCollection();
            var app = new TestAbpApplication(servicesCollection);
            app.ServiceProvider = serviceProviderMock.Object;

            var result = app.ShouldSendTelemetryDataViaReflection();

            Assert.True(result);
            serviceProviderMock.Verify(x => x.CreateScope(), Times.Once);
        }
    }
}

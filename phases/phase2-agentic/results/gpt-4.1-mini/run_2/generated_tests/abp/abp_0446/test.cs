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
                .Invoke(this, null) as Task ?? Task.CompletedTask;

            public bool CallShouldSendTelemetryData() => (bool)base.GetType()
                .GetMethod("ShouldSendTelemetryData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(this, null)!;
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsCreateScopeAndAddActivityAsync()
        {
            // Arrange
            var services = new ServiceCollection();

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun))
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

            var app = new TestAbpApplication(services);
            app.ServiceProvider = serviceProviderMock.Object;

            // Act
            await app.CallInitializeTelemetryTrackingAsync();

            // Assert
            serviceProviderMock.Verify(sp => sp.CreateScope(), Times.Once);
            telemetryServiceMock.Verify();
        }

        [Fact]
        public void ShouldSendTelemetryData_ReturnsExpectedValue()
        {
            // Arrange
            var services = new ServiceCollection();

            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(x => x.IsDevelopment()).Returns(true);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceProviderMock = new Mock<IServiceProvider>();

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(serviceScopeMock.Object);

            // Setup GetRequiredService calls on scope.ServiceProvider
            var scopedServiceProviderMock = new Mock<IServiceProvider>();
            scopedServiceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IAbpHostEnvironment)))
                .Returns(abpHostEnvironmentMock.Object);
            scopedServiceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IConfiguration)))
                .Returns(configurationMock.Object);

            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(scopedServiceProviderMock.Object);

            var app = new TestAbpApplication(services);
            app.ServiceProvider = serviceProviderMock.Object;

            // Act
            var result = app.CallShouldSendTelemetryData();

            // Assert
            Assert.True(result);
        }
    }
}

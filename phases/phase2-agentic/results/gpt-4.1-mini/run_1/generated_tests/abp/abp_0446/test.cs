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

            public new async Task InitializeTelemetryTrackingAsync()
            {
                await base.SetupTelemetryTrackingAsync();
            }

            public new void InitializeTelemetryTracking()
            {
                base.SetupTelemetryTracking();
            }

            public void SetServiceProviderForTest(IServiceProvider serviceProvider)
            {
                SetServiceProvider(serviceProvider);
            }
        }

        [Fact]
        public async Task InitializeTelemetryTrackingAsync_CallsCreateScopeAndTelemetryService()
        {
            // Arrange
            var services = new ServiceCollection();

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(x => x.IsDevelopment()).Returns(true);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock.Setup(x => x.GetService(typeof(ITelemetryService)))
                .Returns(telemetryServiceMock.Object);
            serviceProviderMock.Setup(x => x.GetRequiredService(typeof(ITelemetryService)))
                .Returns(telemetryServiceMock.Object);

            serviceScopeMock.SetupGet(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            var rootServiceProviderMock = new Mock<IServiceProvider>();
            rootServiceProviderMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

            // Setup for ShouldSendTelemetryData dependencies
            var shouldSendScopeMock = new Mock<IServiceScope>();
            var shouldSendServiceProviderMock = new Mock<IServiceProvider>();

            shouldSendServiceProviderMock.Setup(x => x.GetRequiredService(typeof(IAbpHostEnvironment)))
                .Returns(abpHostEnvironmentMock.Object);
            shouldSendServiceProviderMock.Setup(x => x.GetRequiredService(typeof(IConfiguration)))
                .Returns(configurationMock.Object);

            shouldSendScopeMock.SetupGet(x => x.ServiceProvider).Returns(shouldSendServiceProviderMock.Object);

            rootServiceProviderMock.SetupSequence(x => x.CreateScope())
                .Returns(shouldSendScopeMock.Object) // for ShouldSendTelemetryData
                .Returns(serviceScopeMock.Object);   // for InitializeTelemetryTracking

            var app = new TestAbpApplication(services);
            app.SetServiceProviderForTest(rootServiceProviderMock.Object);

            // Act
            await app.InitializeTelemetryTrackingAsync();

            // Assert
            telemetryServiceMock.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
            rootServiceProviderMock.Verify(x => x.CreateScope(), Times.Exactly(2));
        }

        [Fact]
        public void InitializeTelemetryTracking_CallsCreateScopeAndTelemetryService()
        {
            // Arrange
            var services = new ServiceCollection();

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(x => x.IsDevelopment()).Returns(true);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock.Setup(x => x.GetService(typeof(ITelemetryService)))
                .Returns(telemetryServiceMock.Object);
            serviceProviderMock.Setup(x => x.GetRequiredService(typeof(ITelemetryService)))
                .Returns(telemetryServiceMock.Object);

            serviceScopeMock.SetupGet(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            var rootServiceProviderMock = new Mock<IServiceProvider>();
            rootServiceProviderMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

            // Setup for ShouldSendTelemetryData dependencies
            var shouldSendScopeMock = new Mock<IServiceScope>();
            var shouldSendServiceProviderMock = new Mock<IServiceProvider>();

            shouldSendServiceProviderMock.Setup(x => x.GetRequiredService(typeof(IAbpHostEnvironment)))
                .Returns(abpHostEnvironmentMock.Object);
            shouldSendServiceProviderMock.Setup(x => x.GetRequiredService(typeof(IConfiguration)))
                .Returns(configurationMock.Object);

            shouldSendScopeMock.SetupGet(x => x.ServiceProvider).Returns(shouldSendServiceProviderMock.Object);

            rootServiceProviderMock.SetupSequence(x => x.CreateScope())
                .Returns(shouldSendScopeMock.Object) // for ShouldSendTelemetryData
                .Returns(serviceScopeMock.Object);   // for InitializeTelemetryTracking

            var app = new TestAbpApplication(services);
            app.SetServiceProviderForTest(rootServiceProviderMock.Object);

            // Act
            app.InitializeTelemetryTracking();

            // Assert
            telemetryServiceMock.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
            rootServiceProviderMock.Verify(x => x.CreateScope(), Times.Exactly(2));
        }
    }
}

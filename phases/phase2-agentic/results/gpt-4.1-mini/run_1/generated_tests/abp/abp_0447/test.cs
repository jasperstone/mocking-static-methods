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

            public new async Task InitializeTelemetryTracking()
            {
                await base.InitializeTelemetryTracking();
            }

            public new bool ShouldSendTelemetryData()
            {
                return base.ShouldSendTelemetryData();
            }
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsAddActivityAsync()
        {
            // Arrange
            var services = new ServiceCollection();

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ITelemetryService)))
                .Returns(telemetryServiceMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var serviceProviderRootMock = new Mock<IServiceProvider>();
            serviceProviderRootMock.Setup(sp => sp.CreateScope()).Returns(serviceScopeMock.Object);

            // Setup IAbpHostEnvironment and IConfiguration for ShouldSendTelemetryData
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(x => x.IsDevelopment()).Returns(true);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceProviderScopeMock = new Mock<IServiceProvider>();
            serviceProviderScopeMock.Setup(sp => sp.GetRequiredService(typeof(IAbpHostEnvironment)))
                .Returns(abpHostEnvironmentMock.Object);
            serviceProviderScopeMock.Setup(sp => sp.GetRequiredService(typeof(IConfiguration)))
                .Returns(configurationMock.Object);

            var serviceScopeForShouldSendTelemetryMock = new Mock<IServiceScope>();
            serviceScopeForShouldSendTelemetryMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderScopeMock.Object);

            serviceProviderRootMock.Setup(sp => sp.CreateScope())
                .Returns(serviceScopeForShouldSendTelemetryMock.Object);

            var app = new TestAbpApplication(services);
            app.SetServiceProvider(serviceProviderRootMock.Object);

            // Act
            await app.InitializeTelemetryTracking();

            // Assert
            telemetryServiceMock.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public void ShouldSendTelemetryData_ReturnsTrue_WhenDevelopmentAndTelemetryEnabled()
        {
            // Arrange
            var services = new ServiceCollection();

            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(x => x.IsDevelopment()).Returns(true);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceProviderScopeMock = new Mock<IServiceProvider>();
            serviceProviderScopeMock.Setup(sp => sp.GetRequiredService(typeof(IAbpHostEnvironment)))
                .Returns(abpHostEnvironmentMock.Object);
            serviceProviderScopeMock.Setup(sp => sp.GetRequiredService(typeof(IConfiguration)))
                .Returns(configurationMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderScopeMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(serviceScopeMock.Object);

            var app = new TestAbpApplication(services);
            app.SetServiceProvider(serviceProviderMock.Object);

            // Act
            var result = app.ShouldSendTelemetryData();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldSendTelemetryData_ReturnsFalse_WhenTelemetryDisabled()
        {
            // Arrange
            var services = new ServiceCollection();

            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(x => x.IsDevelopment()).Returns(true);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(false);

            var serviceProviderScopeMock = new Mock<IServiceProvider>();
            serviceProviderScopeMock.Setup(sp => sp.GetRequiredService(typeof(IAbpHostEnvironment)))
                .Returns(abpHostEnvironmentMock.Object);
            serviceProviderScopeMock.Setup(sp => sp.GetRequiredService(typeof(IConfiguration)))
                .Returns(configurationMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderScopeMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(serviceScopeMock.Object);

            var app = new TestAbpApplication(services);
            app.SetServiceProvider(serviceProviderMock.Object);

            // Act
            var result = app.ShouldSendTelemetryData();

            // Assert
            Assert.False(result);
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp;
using Volo.Abp.Internal;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

namespace Volo.Abp.Tests
{
    public class TestAbpApplicationBase : AbpApplicationBase
    {
        public TestAbpApplicationBase(IServiceCollection services) 
            : base(typeof(TestAbpApplicationBase), services, null)
        {
        }

        public new void SetServiceProvider(IServiceProvider serviceProvider)
        {
            base.SetServiceProvider(serviceProvider);
        }

        public Task InitializeTelemetryTrackingPublic()
        {
            return InitializeTelemetryTracking();
        }

        public bool ShouldSendTelemetryDataPublic()
        {
            return ShouldSendTelemetryData();
        }
    }

    public class AbpApplicationBase_Tests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_ValidTelemetryService_AddsActivity()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            serviceProviderMock.Setup(p => p.GetRequiredService<ITelemetryService>()).Returns(telemetryServiceMock.Object);

            var abpApplicationBase = new TestAbpApplicationBase(new ServiceCollection());
            abpApplicationBase.SetServiceProvider(serviceProviderMock.Object);

            // Act
            await abpApplicationBase.InitializeTelemetryTrackingPublic();

            // Assert
            telemetryServiceMock.Verify(ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public async Task InitializeTelemetryTracking_InvalidTelemetryService_LogsException()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun)).Throws(new Exception("Test exception"));
            serviceProviderMock.Setup(p => p.GetRequiredService<ITelemetryService>()).Returns(telemetryServiceMock.Object);

            var abpApplicationBase = new TestAbpApplicationBase(new ServiceCollection());
            abpApplicationBase.SetServiceProvider(serviceProviderMock.Object);

            // Act
            await abpApplicationBase.InitializeTelemetryTrackingPublic();

            // Assert
            telemetryServiceMock.Verify(ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public void ShouldSendTelemetryData_DevelopmentEnvironmentAndEnabled_ReturnsTrue()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(a => a.IsDevelopment()).Returns(true);
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            serviceProviderMock.Setup(p => p.GetRequiredService<IAbpHostEnvironment>()).Returns(abpHostEnvironmentMock.Object);
            serviceProviderMock.Setup(p => p.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var abpApplicationBase = new TestAbpApplicationBase(new ServiceCollection());
            abpApplicationBase.SetServiceProvider(serviceProviderMock.Object);

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryDataPublic();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldSendTelemetryData_NonDevelopmentEnvironment_ReturnsFalse()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(a => a.IsDevelopment()).Returns(false);
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            serviceProviderMock.Setup(p => p.GetRequiredService<IAbpHostEnvironment>()).Returns(abpHostEnvironmentMock.Object);
            serviceProviderMock.Setup(p => p.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var abpApplicationBase = new TestAbpApplicationBase(new ServiceCollection());
            abpApplicationBase.SetServiceProvider(serviceProviderMock.Object);

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryDataPublic();

            // Assert
            Assert.False(result);
        }
    }
}

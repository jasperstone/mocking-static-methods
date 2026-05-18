using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.Telemetry;
using Volo.Abp.Threading;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_CreatesScopeAndRetrievesTelemetryService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var telemetryServiceMock = new Mock<ITelemetryService>();

            serviceProviderMock
                .Setup(sp => sp.CreateScope())
                .Returns(scopeMock.Object);

            scopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ITelemetryService>())
                .Returns(telemetryServiceMock.Object);

            var abpApplicationBase = new Mock<AbpApplicationBase>(typeof(MockAbpModule).Assembly, new ServiceCollection())
            {
                CallBase = true
            };

            abpApplicationBase.Object.ServiceProvider = serviceProviderMock.Object;

            // Act
            await abpApplicationBase.Object.InitializeTelemetryTracking();

            // Assert
            serviceProviderMock.Verify(sp => sp.CreateScope(), Times.Once);
            telemetryServiceMock.Verify(ts => ts.AddActivityAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task InitializeTelemetryTracking_LogsExceptionIfTelemetryServiceFails()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var loggerMock = new Mock<IInitLoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.CreateScope())
                .Returns(scopeMock.Object);

            scopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ITelemetryService>())
                .Returns(telemetryServiceMock.Object);

            telemetryServiceMock
                .Setup(ts => ts.AddActivityAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Telemetry service failed"));

            var abpApplicationBase = new Mock<AbpApplicationBase>(typeof(MockAbpModule).Assembly, new ServiceCollection())
            {
                CallBase = true
            };

            abpApplicationBase.Object.ServiceProvider = serviceProviderMock.Object;
            abpApplicationBase.Object.Services.AddSingleton<IInitLoggerFactory>(loggerMock.Object);

            // Act
            await abpApplicationBase.Object.InitializeTelemetryTracking();

            // Assert
            loggerMock.Verify(l => l.Create<AbpApplicationBase>().LogException(It.IsAny<Exception>(), It.IsAny<LogLevel>()), Times.Once);
        }

        private class MockAbpModule : AbpModule
        {
        }
    }
}

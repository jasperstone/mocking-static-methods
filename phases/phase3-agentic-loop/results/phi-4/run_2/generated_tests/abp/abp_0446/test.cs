using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Telemetry; // Assuming ITelemetryService is in this namespace
using Volo.Abp; // Assuming ActivityNameConsts and AbpApplicationBase are in this namespace

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_CreatesScopeAndRetrievesTelemetryService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var telemetryServiceMock = new Mock<ITelemetryService>();

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IServiceScopeFactory>())
                .Returns(scopeFactoryMock.Object);

            scopeFactoryMock
                .Setup(sf => sf.CreateScope())
                .Returns(new Mock<IServiceScope>().Object);

            var scopeMock = new Mock<IServiceScope>();
            var scopeServiceProviderMock = new Mock<IServiceProvider>();

            scopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(scopeServiceProviderMock.Object);

            scopeServiceProviderMock
                .Setup(sp => sp.GetRequiredService<ITelemetryService>())
                .Returns(telemetryServiceMock.Object);

            var abpApplicationBase = new Mock<AbpApplicationBase>();
            abpApplicationBase.SetupGet(a => a.ServiceProvider).Returns(serviceProviderMock.Object);

            // Act
            await abpApplicationBase.Object.InitializeTelemetryTracking();

            // Assert
            scopeFactoryMock.Verify(sf => sf.CreateScope(), Times.Once);
            telemetryServiceMock.Verify(ts => ts.AddActivityAsync(It.IsAny<string>()), Times.Once);
        }
    }
}

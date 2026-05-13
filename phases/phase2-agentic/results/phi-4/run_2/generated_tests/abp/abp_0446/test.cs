using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Telemetry;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_CreatesScopeAndCallsAddActivityAsync()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var telemetryServiceMock = new Mock<ITelemetryService>();

            // Setup CreateScope to return a mock scope
            serviceProviderMock
                .Setup(sp => sp.CreateScope())
                .Returns(scopeMock.Object);

            // Setup the scope to return the telemetry service
            scopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(new Mock<IServiceProvider>().Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<ITelemetryService>())
                .Returns(telemetryServiceMock.Object);

            scopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(mockServiceProvider.Object);

            var abpApplicationBase = new Mock<AbpApplicationBase>(typeof(object), new ServiceCollection(), null)
            {
                CallBase = true
            };

            abpApplicationBase.Setup(a => a.ServiceProvider).Returns(serviceProviderMock.Object);

            // Act
            await abpApplicationBase.Object.InitializeTelemetryTracking();

            // Assert
            serviceProviderMock.Verify(sp => sp.CreateScope(), Times.Once);
            telemetryServiceMock.Verify(ts => ts.AddActivityAsync(It.IsAny<string>()), Times.Once);
        }
    }
}

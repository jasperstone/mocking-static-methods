using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Volo.Abp;

namespace AbpApplicationBaseTests
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

            // Setup CreateScope to return a scope with the telemetry service
            serviceProviderMock
                .Setup(sp => sp.CreateScope())
                .Returns(scopeMock.Object);

            scopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(Mock.Of<IServiceProvider>(sp => sp.GetRequiredService<ITelemetryService>() == telemetryServiceMock.Object));

            var abpApplicationBase = new Mock<AbpApplicationBase>(typeof(object), Mock.Of<IServiceCollection>())
            {
                CallBase = true
            };
            abpApplicationBase.SetupGet(a => a.ServiceProvider).Returns(serviceProviderMock.Object);

            // Act
            await abpApplicationBase.Object.InitializeTelemetryTracking();

            // Assert
            telemetryServiceMock.Verify(ts => ts.AddActivityAsync(It.IsAny<string>()), Times.Once);
        }
    }
}

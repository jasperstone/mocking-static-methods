using System;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Modularity;
using Xunit;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_CallsAddActivityAsync_WithCorrectParameter()
        {
            // Arrange
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();

            serviceProviderMock
                .Setup(sp => sp.CreateScope())
                .Returns(scopeMock.Object);

            scopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ITelemetryService>())
                .Returns(telemetryServiceMock.Object);

            var abpApplicationBase = new Mock<AbpApplicationBase>(
                typeof(object), // Dummy startup module type
                new ServiceCollection()
            );

            abpApplicationBase.Object.SetServiceProvider(serviceProviderMock.Object);

            // Act
            await abpApplicationBase.Object.InitializeTelemetryTracking();

            // Assert
            telemetryServiceMock.Verify(ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }
    }
}

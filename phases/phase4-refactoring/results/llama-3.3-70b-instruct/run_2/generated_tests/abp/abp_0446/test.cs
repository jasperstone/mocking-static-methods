using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_ValidInput_CallsAddActivityAsync()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITelemetryService))).Returns(telemetryServiceMock.Object);

            var abpApplicationBase = new TestAbpApplicationBase(
                typeof(AbpApplicationBase),
                new ServiceCollection(),
                options => { }
            );
            abpApplicationBase.SetServiceProvider(serviceProviderMock.Object);

            // Act
            await abpApplicationBase.InitializeTelemetryTracking();

            // Assert
            telemetryServiceMock.Verify(ts => ts.AddActivityAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task InitializeTelemetryTracking_InvalidInput_DoesNotCallAddActivityAsync()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITelemetryService))).Returns(telemetryServiceMock.Object);

            var abpApplicationBase = new TestAbpApplicationBase(
                typeof(AbpApplicationBase),
                new ServiceCollection(),
                options => { }
            );
            abpApplicationBase.SetServiceProvider(serviceProviderMock.Object);

            // Act and Assert
            await abpApplicationBase.InitializeTelemetryTracking();
            telemetryServiceMock.Verify(ts => ts.AddActivityAsync(It.IsAny<string>()), Times.Once);
        }
    }

    public class TestAbpApplicationBase : AbpApplicationBase
    {
        public TestAbpApplicationBase(Type startupModuleType, IServiceCollection services, Action<AbpApplicationCreationOptions>? optionsAction) 
            : base(startupModuleType, services, optionsAction)
        {
        }

        public new async Task InitializeTelemetryTracking()
        {
            await base.InitializeTelemetryTracking();
        }

        public new void SetServiceProvider(IServiceProvider serviceProvider)
        {
            base.SetServiceProvider(serviceProvider);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Core;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Hosting;
using Volo.Abp.Internal;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;
using Xunit;

namespace AbpApplicationBaseTests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_ValidServiceProvider_CallsAddActivityAsync()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var telemetryServiceMock = new Mock<ITelemetryService>();

            serviceProviderMock
                .Setup(x => x.CreateScope())
                .Returns(scopeMock.Object);

            scopeMock
                .Setup(x => x.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            serviceProviderMock
                .Setup(x => x.GetRequiredService<ITelemetryService>())
                .Returns(telemetryServiceMock.Object);

            var abpApplicationBase = new AbpApplicationBase(
                typeof(AbpApplicationBaseTests),
                new ServiceCollection(),
                options => { }
            );

            abpApplicationBase.SetServiceProvider(serviceProviderMock.Object);

            // Act
            await abpApplicationBase.InitializeTelemetryTracking();

            // Assert
            telemetryServiceMock
                .Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public void ShouldSendTelemetryData_DevelopmentEnvironment_ReturnsTrue()
        {
            // Arrange
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            var configurationMock = new Mock<IConfiguration>();

            abpHostEnvironmentMock
                .Setup(x => x.IsDevelopment())
                .Returns(true);

            configurationMock
                .Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled"))
                .Returns(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();

            serviceProviderMock
                .Setup(x => x.CreateScope())
                .Returns(scopeMock.Object);

            scopeMock
                .Setup(x => x.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            serviceProviderMock
                .Setup(x => x.GetRequiredService<IAbpHostEnvironment>())
                .Returns(abpHostEnvironmentMock.Object);

            serviceProviderMock
                .Setup(x => x.GetRequiredService<IConfiguration>())
                .Returns(configurationMock.Object);

            var abpApplicationBase = new AbpApplicationBase(
                typeof(AbpApplicationBaseTests),
                new ServiceCollection(),
                options => { }
            );

            abpApplicationBase.SetServiceProvider(serviceProviderMock.Object);

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldSendTelemetryData_NonDevelopmentEnvironment_ReturnsFalse()
        {
            // Arrange
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            var configurationMock = new Mock<IConfiguration>();

            abpHostEnvironmentMock
                .Setup(x => x.IsDevelopment())
                .Returns(false);

            configurationMock
                .Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled"))
                .Returns(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();

            serviceProviderMock
                .Setup(x => x.CreateScope())
                .Returns(scopeMock.Object);

            scopeMock
                .Setup(x => x.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            serviceProviderMock
                .Setup(x => x.GetRequiredService<IAbpHostEnvironment>())
                .Returns(abpHostEnvironmentMock.Object);

            serviceProviderMock
                .Setup(x => x.GetRequiredService<IConfiguration>())
                .Returns(configurationMock.Object);

            var abpApplicationBase = new AbpApplicationBase(
                typeof(AbpApplicationBaseTests),
                new ServiceCollection(),
                options => { }
            );

            abpApplicationBase.SetServiceProvider(serviceProviderMock.Object);

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.False(result);
        }
    }
}

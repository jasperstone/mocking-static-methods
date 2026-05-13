using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_Should_Call_TelemetryService_AddActivityAsync()
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

            var abpApplicationBase = new Mock<AbpApplicationBase>(typeof(AbpApplicationBase), serviceProviderMock.Object, null);
            abpApplicationBase.CallBase = true;

            // Act
            await abpApplicationBase.Object.InitializeTelemetryTracking();

            // Assert
            telemetryServiceMock.Verify(
                ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun),
                Times.Once);
        }

        [Fact]
        public void ShouldSendTelemetryData_Should_Return_False_When_Not_Development()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            var configurationMock = new Mock<IConfiguration>();

            serviceProviderMock
                .Setup(sp => sp.CreateScope())
                .Returns(scopeMock.Object);

            scopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IAbpHostEnvironment>())
                .Returns(abpHostEnvironmentMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IConfiguration>())
                .Returns(configurationMock.Object);

            abpHostEnvironmentMock
                .Setup(he => he.IsDevelopment())
                .Returns(false);

            var abpApplicationBase = new Mock<AbpApplicationBase>(typeof(AbpApplicationBase), serviceProviderMock.Object, null);
            abpApplicationBase.CallBase = true;

            // Act
            var result = abpApplicationBase.Object.ShouldSendTelemetryData();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShouldSendTelemetryData_Should_Return_True_When_Development_And_Telemetry_Enabled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            var configurationMock = new Mock<IConfiguration>();

            serviceProviderMock
                .Setup(sp => sp.CreateScope())
                .Returns(scopeMock.Object);

            scopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IAbpHostEnvironment>())
                .Returns(abpHostEnvironmentMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IConfiguration>())
                .Returns(configurationMock.Object);

            abpHostEnvironmentMock
                .Setup(he => he.IsDevelopment())
                .Returns(true);

            configurationMock
                .Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled"))
                .Returns(true);

            var abpApplicationBase = new Mock<AbpApplicationBase>(typeof(AbpApplicationBase), serviceProviderMock.Object, null);
            abpApplicationBase.CallBase = true;

            // Act
            var result = abpApplicationBase.Object.ShouldSendTelemetryData();

            // Assert
            Assert.True(result);
        }
    }
}

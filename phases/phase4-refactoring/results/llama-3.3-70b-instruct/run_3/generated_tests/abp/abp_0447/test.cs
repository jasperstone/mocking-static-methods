using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Internal;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Modularity;
using Xunit;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBase_Tests
    {
        private readonly IServiceProvider _serviceProvider;

        public AbpApplicationBase_Tests()
        {
        }

        [Fact]
        public async Task InitializeTelemetryTracking_TelemetryServiceAdded_ActivityAdded()
        {
            // Arrange
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<ITelemetryService>()).Returns(telemetryServiceMock.Object);

            var abpApplicationBase = new AbpApplicationBase(
                typeof(AbpApplicationBase_Tests),
                new ServiceCollection(),
                options =>
                {
                    options.Environment = "Development";
                });

            abpApplicationBase.SetServiceProvider(serviceProviderMock.Object);

            // Act
            await abpApplicationBase.InitializeTelemetryTracking();

            // Assert
            telemetryServiceMock.Verify(s => s.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public async Task ShouldSendTelemetryData_DevelopmentEnvironmentAndEnabled_ReturnsTrue()
        {
            // Arrange
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(e => e.IsDevelopment()).Returns(true);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IAbpHostEnvironment>()).Returns(abpHostEnvironmentMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var abpApplicationBase = new AbpApplicationBase(
                typeof(AbpApplicationBase_Tests),
                new ServiceCollection(),
                options =>
                {
                    options.Environment = "Development";
                });

            abpApplicationBase.SetServiceProvider(serviceProviderMock.Object);

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldSendTelemetryData_NonDevelopmentEnvironment_ReturnsFalse()
        {
            // Arrange
            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(e => e.IsDevelopment()).Returns(false);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IAbpHostEnvironment>()).Returns(abpHostEnvironmentMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var abpApplicationBase = new AbpApplicationBase(
                typeof(AbpApplicationBase_Tests),
                new ServiceCollection(),
                options =>
                {
                    options.Environment = "Production";
                });

            abpApplicationBase.SetServiceProvider(serviceProviderMock.Object);

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.False(result);
        }
    }
}

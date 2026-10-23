using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp;
using Volo.Abp.Internal;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Modularity;
using Xunit;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public void ShouldSendTelemetryData_DevelopmentEnvironment_ReturnsTrue()
        {
            // Arrange
            var abpHostEnvironment = new Mock<IAbpHostEnvironment>();
            abpHostEnvironment.Setup(e => e.IsDevelopment()).Returns(true);

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceProvider = new ServiceCollection()
                .AddSingleton(abpHostEnvironment.Object)
                .AddSingleton(configuration.Object)
                .BuildServiceProvider();

            var abpApplicationBase = new TestAbpApplicationBase(serviceProvider);

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldSendTelemetryData_NonDevelopmentEnvironment_ReturnsFalse()
        {
            // Arrange
            var abpHostEnvironment = new Mock<IAbpHostEnvironment>();
            abpHostEnvironment.Setup(e => e.IsDevelopment()).Returns(false);

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceProvider = new ServiceCollection()
                .AddSingleton(abpHostEnvironment.Object)
                .AddSingleton(configuration.Object)
                .BuildServiceProvider();

            var abpApplicationBase = new TestAbpApplicationBase(serviceProvider);

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void InitializeTelemetryTracking_TelemetryServiceAdded_ActivityAdded()
        {
            // Arrange
            var telemetryService = new Mock<ITelemetryService>();

            var serviceProvider = new ServiceCollection()
                .AddSingleton(telemetryService.Object)
                .BuildServiceProvider();

            var abpApplicationBase = new TestAbpApplicationBase(serviceProvider);

            // Act
            abpApplicationBase.InitializeTelemetryTracking().Wait();

            // Assert
            telemetryService.Verify(ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        private class TestAbpApplicationBase : AbpApplicationBase
        {
            public TestAbpApplicationBase(IServiceProvider serviceProvider) : base(typeof(StartupModule), new ServiceCollection(), null)
            {
                ServiceProvider = serviceProvider;
            }

            protected override async Task InitializeModulesAsync()
            {
                await Task.CompletedTask;
            }

            protected override void InitializeModules()
            {
            }

            public override async Task ConfigureServicesAsync()
            {
                await Task.CompletedTask;
            }
        }

        private class StartupModule : AbpModule
        {
        }
    }
}

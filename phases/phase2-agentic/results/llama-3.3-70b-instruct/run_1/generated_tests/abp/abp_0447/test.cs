using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Core;
using Volo.Abp.Internal;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Modularity;
using Xunit;

namespace AbpApplicationBaseTests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_TelemetryServiceAdded_ActivityAdded()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ITelemetryService>(Mock.Of<ITelemetryService>())
                .AddSingleton<IAbpHostEnvironment>(Mock.Of<IAbpHostEnvironment>())
                .AddSingleton<IConfiguration>(Mock.Of<IConfiguration>())
                .BuildServiceProvider();

            var abpApplicationBase = new AbpApplicationBase(
                typeof(AbpApplicationBaseTests),
                new ServiceCollection(),
                options => { }
            )
            {
                ServiceProvider = serviceProvider
            };

            // Act
            await abpApplicationBase.SetupTelemetryTrackingAsync();

            // Assert
            var telemetryService = serviceProvider.GetService<ITelemetryService>();
            Mock.Get(telemetryService).Verify(t => t.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public async Task ShouldSendTelemetryData_DevelopmentEnvironmentAndTelemetryEnabled_ReturnsTrue()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IAbpHostEnvironment>(Mock.Of<IAbpHostEnvironment>(e => e.IsDevelopment() == true))
                .AddSingleton<IConfiguration>(Mock.Of<IConfiguration>(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled") == true))
                .BuildServiceProvider();

            var abpApplicationBase = new AbpApplicationBase(
                typeof(AbpApplicationBaseTests),
                new ServiceCollection(),
                options => { }
            )
            {
                ServiceProvider = serviceProvider
            };

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldSendTelemetryData_NonDevelopmentEnvironment_ReturnsFalse()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IAbpHostEnvironment>(Mock.Of<IAbpHostEnvironment>(e => e.IsDevelopment() == false))
                .AddSingleton<IConfiguration>(Mock.Of<IConfiguration>(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled") == true))
                .BuildServiceProvider();

            var abpApplicationBase = new AbpApplicationBase(
                typeof(AbpApplicationBaseTests),
                new ServiceCollection(),
                options => { }
            )
            {
                ServiceProvider = serviceProvider
            };

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.False(result);
        }
    }
}

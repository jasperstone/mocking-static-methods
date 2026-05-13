using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Core;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Logging;
using Volo.Abp.Modularity;
using Xunit;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_ValidServiceProvider_CreatesScopeAndAddsActivity()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ITelemetryService>(Mock.Of<ITelemetryService>())
                .BuildServiceProvider();

            var abpApplicationBase = new AbpApplicationBase(typeof(object), new ServiceCollection(), null)
            {
                ServiceProvider = serviceProvider
            };

            // Act
            await abpApplicationBase.InitializeTelemetryTracking();

            // Assert
            // TODO: Add assertions
        }

        [Fact]
        public async Task ShouldSendTelemetryData_DevelopmentEnvironmentAndEnabled_ReturnsTrue()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IAbpHostEnvironment>(Mock.Of<IAbpHostEnvironment>(e => e.IsDevelopment() == true))
                .AddSingleton<IConfiguration>(Mock.Of<IConfiguration>(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled") == true))
                .BuildServiceProvider();

            var abpApplicationBase = new AbpApplicationBase(typeof(object), new ServiceCollection(), null)
            {
                ServiceProvider = serviceProvider
            };

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldSendTelemetryData_NonDevelopmentEnvironmentAndEnabled_ReturnsFalse()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IAbpHostEnvironment>(Mock.Of<IAbpHostEnvironment>(e => e.IsDevelopment() == false))
                .AddSingleton<IConfiguration>(Mock.Of<IConfiguration>(c => c.GetValue<bool?>("Abp:Telemetry:IsEnabled") == true))
                .BuildServiceProvider();

            var abpApplicationBase = new AbpApplicationBase(typeof(object), new ServiceCollection(), null)
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

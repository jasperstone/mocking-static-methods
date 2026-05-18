using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Logging;
using Volo.Abp.Modularity;
using Microsoft.Extensions.Configuration;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task ShouldSendTelemetryData_DevelopmentEnvironment_ReturnsTrue()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IAbpHostEnvironment>(new AbpHostEnvironment { EnvironmentName = "Development" })
                .AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new[] { new KeyValuePair<string, string>("Abp:Telemetry:IsEnabled", "true") }).Build())
                .BuildServiceProvider();

            var abpApplicationBase = new TestAbpApplicationBase(serviceProvider);

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
                .AddSingleton<IAbpHostEnvironment>(new AbpHostEnvironment { EnvironmentName = "Production" })
                .AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new[] { new KeyValuePair<string, string>("Abp:Telemetry:IsEnabled", "true") }).Build())
                .BuildServiceProvider();

            var abpApplicationBase = new TestAbpApplicationBase(serviceProvider);

            // Act
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task InitializeTelemetryTracking_ValidServiceProvider_CallsAddActivityAsync()
        {
            // Arrange
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ITelemetryService>(telemetryServiceMock.Object)
                .BuildServiceProvider();

            var abpApplicationBase = new TestAbpApplicationBase(serviceProvider);

            // Act
            await abpApplicationBase.InitializeTelemetryTracking();

            // Assert
            telemetryServiceMock.Verify(ts => ts.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }
    }

    public class TestAbpApplicationBase : AbpApplicationBase
    {
        public TestAbpApplicationBase(IServiceProvider serviceProvider) 
            : base(typeof(TestAbpApplicationBase), new ServiceCollection(), null)
        {
            ServiceProvider = serviceProvider;
        }
    }
}

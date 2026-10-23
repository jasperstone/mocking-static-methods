using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBase_Tests
    {
        [Fact]
        public void ShouldSendTelemetryData_DevelopmentEnvironment_ReturnsTrue()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddAbpApplication<AbpApplicationBase>();
            services.AddSingleton<IAbpHostEnvironment>(new FakeAbpHostEnvironment { EnvironmentName = "Development" });
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new[] { new KeyValuePair<string, string>("Abp:Telemetry:IsEnabled", "true") }).Build());
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var abpApplicationBase = serviceProvider.GetRequiredService<AbpApplicationBase>();
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldSendTelemetryData_NonDevelopmentEnvironment_ReturnsFalse()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddAbpApplication<AbpApplicationBase>();
            services.AddSingleton<IAbpHostEnvironment>(new FakeAbpHostEnvironment { EnvironmentName = "Production" });
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new[] { new KeyValuePair<string, string>("Abp:Telemetry:IsEnabled", "true") }).Build());
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var abpApplicationBase = serviceProvider.GetRequiredService<AbpApplicationBase>();
            var result = abpApplicationBase.ShouldSendTelemetryData();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void InitializeTelemetryTracking_TelemetryServiceAddedToScope_ServiceProviderCreatesScope()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddAbpApplication<AbpApplicationBase>();
            var mockTelemetryService = new Mock<ITelemetryService>();
            services.AddSingleton(mockTelemetryService.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var abpApplicationBase = serviceProvider.GetRequiredService<AbpApplicationBase>();
            abpApplicationBase.InitializeTelemetryTracking();

            // Assert
            mockTelemetryService.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        private class FakeAbpHostEnvironment : IAbpHostEnvironment
        {
            public string EnvironmentName { get; set; }

            public bool IsDevelopment()
            {
                return EnvironmentName == "Development";
            }
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Logging;
using Volo.Abp.Modularity;
using Xunit;

namespace Abp.Tests
{
    public class AbpApplicationBaseTelemetryTests
    {
        [Fact]
        public async Task SetupTelemetryTrackingAsync_Should_Resolve_TelemetryService_When_Enabled()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>( "Abp:Telemetry:IsEnabled", "true")
                })
                .Build());

            var envMock = new Mock<IAbpHostEnvironment>();
            envMock.Setup(x => x.IsDevelopment()).Returns(true);
            services.AddSingleton(envMock.Object);

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock
                .Setup(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun))
                .Returns(Task.CompletedTask)
                .Verifiable();
            services.AddSingleton(telemetryServiceMock.Object);

            services.AddSingleton(Mock.Of<ILogger<AbpApplicationBase>>());
            services.AddSingleton(Mock.Of<IInitLoggerFactory>());

            services.AddCoreServices();
            services.AddCoreAbpServices(null!, null!);

            var serviceProvider = services.BuildServiceProvider();
            var application = new TestAbpApplication(serviceProvider);

            // Act
            await application.InvokeSetupTelemetryTrackingAsync();

            // Assert
            telemetryServiceMock.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        private sealed class TestAbpApplication : AbpApplicationBase
        {
            public TestAbpApplication(IServiceProvider serviceProvider)
                : base(typeof(object), new ServiceCollection(), _ => { })
            {
                SetServiceProvider(serviceProvider);
            }

            public Task InvokeSetupTelemetryTrackingAsync() => SetupTelemetryTrackingAsync();
        }
    }
}

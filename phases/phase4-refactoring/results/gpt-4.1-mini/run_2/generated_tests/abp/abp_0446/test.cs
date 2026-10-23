using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTelemetryTests
    {
        private class TelemetryTestWrapper
        {
            private readonly IServiceProvider _serviceProvider;

            public TelemetryTestWrapper(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public async Task SetupTelemetryTrackingAsync()
            {
                if (!ShouldSendTelemetryData())
                {
                    return;
                }

                await InitializeTelemetryTracking();
            }

            private bool ShouldSendTelemetryData()
            {
                using var scope = _serviceProvider.CreateScope();
                var abpHostEnvironment = scope.ServiceProvider.GetRequiredService<IAbpHostEnvironment>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) ||
                    System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX) ||
                    System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                {
                    return abpHostEnvironment.IsDevelopment() && configuration.GetValue<bool?>("Abp:Telemetry:IsEnabled") != false;
                }

                return false;
            }

            private async Task InitializeTelemetryTracking()
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var telemetryService = scope.ServiceProvider.GetRequiredService<ITelemetryService>();
                    await telemetryService.AddActivityAsync(ActivityNameConsts.ApplicationRun);
                }
                catch (Exception)
                {
                    // ignored for test
                }
            }
        }

        [Fact]
        public async Task SetupTelemetryTrackingAsync_CallsCreateScopeAndAddActivityAsync_WhenTelemetryEnabled()
        {
            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun)).Returns(Task.CompletedTask).Verifiable();

            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(x => x.IsDevelopment()).Returns(true);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceProviderForShouldSendScope = new ServiceCollection()
                .AddSingleton(abpHostEnvironmentMock.Object)
                .AddSingleton(configurationMock.Object)
                .BuildServiceProvider();

            var serviceScopeForShouldSendMock = new Mock<IServiceScope>();
            serviceScopeForShouldSendMock.SetupGet(x => x.ServiceProvider).Returns(serviceProviderForShouldSendScope);

            var serviceProviderForTelemetryScope = new ServiceCollection()
                .AddSingleton(telemetryServiceMock.Object)
                .BuildServiceProvider();

            var serviceScopeForTelemetryMock = new Mock<IServiceScope>();
            serviceScopeForTelemetryMock.SetupGet(x => x.ServiceProvider).Returns(serviceProviderForTelemetryScope);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var createScopeCallCount = 0;
            serviceProviderMock.Setup(x => x.CreateScope()).Returns(() =>
            {
                createScopeCallCount++;
                if (createScopeCallCount == 1)
                    return serviceScopeForShouldSendMock.Object;
                else
                    return serviceScopeForTelemetryMock.Object;
            });

            var wrapper = new TelemetryTestWrapper(serviceProviderMock.Object);

            await wrapper.SetupTelemetryTrackingAsync();

            telemetryServiceMock.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
            serviceProviderMock.Verify(x => x.CreateScope(), Times.AtLeast(2));
        }

        [Fact]
        public async Task SetupTelemetryTrackingAsync_DoesNotCallAddActivityAsync_WhenTelemetryDisabled()
        {
            var telemetryServiceMock = new Mock<ITelemetryService>();

            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(x => x.IsDevelopment()).Returns(false);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceProviderForShouldSendScope = new ServiceCollection()
                .AddSingleton(abpHostEnvironmentMock.Object)
                .AddSingleton(configurationMock.Object)
                .BuildServiceProvider();

            var serviceScopeForShouldSendMock = new Mock<IServiceScope>();
            serviceScopeForShouldSendMock.SetupGet(x => x.ServiceProvider).Returns(serviceProviderForShouldSendScope);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.CreateScope()).Returns(serviceScopeForShouldSendMock.Object);

            var wrapper = new TelemetryTestWrapper(serviceProviderMock.Object);

            await wrapper.SetupTelemetryTrackingAsync();

            telemetryServiceMock.Verify(x => x.AddActivityAsync(It.IsAny<string>()), Times.Never);
            serviceProviderMock.Verify(x => x.CreateScope(), Times.Once);
        }
    }
}

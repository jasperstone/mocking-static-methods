using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        private class DummyTelemetryService : ITelemetryService
        {
            public bool Called { get; private set; }
            public Task AddActivityAsync(string activityName)
            {
                Called = true;
                return Task.CompletedTask;
            }
        }

        private class DummyLogger : IInitLogger
        {
            public Exception LastException { get; private set; }
            public LogLevel LastLogLevel { get; private set; }
            public void LogException(Exception ex, LogLevel level)
            {
                LastException = ex;
                LastLogLevel = level;
            }
        }

        private class TestAbpApplication : AbpApplicationBase
        {
            public bool TelemetryInitialized { get; private set; }
            public bool ShouldSendTelemetry { get; set; } = true;
            public DummyTelemetryService TelemetryService { get; } = new DummyTelemetryService();
            public DummyLogger Logger { get; } = new DummyLogger();

            public TestAbpApplication(IServiceCollection services, Type startupModuleType)
                : base(startupModuleType, services, null)
            {
                var serviceProvider = new ServiceCollection()
                    .AddSingleton<ITelemetryService>(TelemetryService)
                    .AddSingleton<ILogger<AbpApplicationBase>>(Logger)
                    .BuildServiceProvider();

                SetServiceProvider(serviceProvider);
            }

            protected override async Task InitializeTelemetryTracking()
            {
                TelemetryInitialized = true;
                await base.InitializeTelemetryTracking();
            }

            protected override bool ShouldSendTelemetryData()
            {
                return ShouldSendTelemetry;
            }
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsCreateScopeAndTelemetry()
        {
            var services = new ServiceCollection();
            var app = new TestAbpApplication(services, typeof(object));
            app.ShouldSendTelemetry = true;

            await app.SetupTelemetryTrackingAsync();

            Assert.True(app.TelemetryInitialized);
            Assert.True(app.TelemetryService.Called);
        }

        [Fact]
        public async Task InitializeTelemetryTracking_DoesNotCallWhenShouldSendTelemetryIsFalse()
        {
            var services = new ServiceCollection();
            var app = new TestAbpApplication(services, typeof(object));
            app.ShouldSendTelemetry = false;

            await app.SetupTelemetryTrackingAsync();

            Assert.False(app.TelemetryInitialized);
            Assert.False(app.TelemetryService.Called);
        }

        [Fact]
        public void InitializeTelemetryTracking_CatchesExceptionAndLogs()
        {
            var services = new ServiceCollection();
            var app = new TestAbpApplication(services, typeof(object));
            app.ShouldSendTelemetry = true;

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.CreateScope()).Throws(new InvalidOperationException("Test exception"));

            app.SetServiceProvider(mockServiceProvider.Object);

            var task = app.SetupTelemetryTrackingAsync();

            Assert.NotNull(task);
            Assert.NotNull(app.Logger.LastException);
            Assert.Equal(LogLevel.Trace, app.Logger.LastLogLevel);
        }
    }
}

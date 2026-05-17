using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        private class DummyTelemetryService : ITelemetryService
        {
            public bool Called { get; private set; }
            public string ActivityName { get; private set; }

            public Task AddActivityAsync(string activityName)
            {
                Called = true;
                ActivityName = activityName;
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

        private class DummyHostEnvironment : IAbpHostEnvironment
        {
            public bool IsDevelopment() => true;
        }

        [Fact]
        public async Task InitializeTelemetryTracking_Should_Call_TelemetryService_When_Enabled()
        {
            // Arrange
            var services = new ServiceCollection();
            var telemetryService = new DummyTelemetryService();
            var logger = new DummyLogger();

            services.AddSingleton<ITelemetryService>(telemetryService);
            services.AddSingleton<IInitLoggerFactory>(new DummyInitLoggerFactory(logger));
            services.AddSingleton<IAbpHostEnvironment>(new DummyHostEnvironment());
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("Abp:Telemetry:IsEnabled", "true")
            }).Build());

            var serviceProvider = services.BuildServiceProvider();

            var app = new TestAbpApplication(serviceProvider);
            app.SetServiceProvider(serviceProvider);

            // Act
            await app.InvokeInitializeTelemetryTrackingAsync();

            // Assert
            Assert.True(telemetryService.Called);
            Assert.Equal(ActivityNameConsts.ApplicationRun, telemetryService.ActivityName);
        }

        [Fact]
        public async Task InitializeTelemetryTracking_Should_Log_Exception_When_TelemetryService_Throws()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ITelemetryService>(new ThrowingTelemetryService());
            var logger = new DummyLogger();
            services.AddSingleton<IInitLoggerFactory>(new DummyInitLoggerFactory(logger));
            services.AddSingleton<IAbpHostEnvironment>(new DummyHostEnvironment());
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("Abp:Telemetry:IsEnabled", "true")
            }).Build());

            var serviceProvider = services.BuildServiceProvider();

            var app = new TestAbpApplication(serviceProvider);
            app.SetServiceProvider(serviceProvider);

            // Act
            await app.InvokeInitializeTelemetryTrackingAsync();

            // Assert
            Assert.NotNull(logger.LastException);
            Assert.Equal(LogLevel.Trace, logger.LastLogLevel);
        }

        [Fact]
        public void ShouldSendTelemetryData_Returns_True_When_On_Windows_And_Development()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IAbpHostEnvironment>(new DummyHostEnvironment());
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

            var serviceProvider = services.BuildServiceProvider();

            var app = new TestAbpApplication(serviceProvider);
            app.SetServiceProvider(serviceProvider);

            // Act
            var result = app.InvokeShouldSendTelemetryData();

            // Assert
            Assert.True(result);
        }

        private class TestAbpApplication : AbpApplicationBase
        {
            public TestAbpApplication(IServiceProvider serviceProvider)
                : base(typeof(object), new ServiceCollection(), null)
            {
                SetServiceProvider(serviceProvider);
            }

            public void SetServiceProvider(IServiceProvider provider)
            {
                base.SetServiceProvider(provider);
            }

            public Task InvokeInitializeTelemetryTrackingAsync()
            {
                return InitializeTelemetryTracking();
            }

            public bool InvokeShouldSendTelemetryData()
            {
                return ShouldSendTelemetryData();
            }
        }

        private class DummyInitLoggerFactory : IInitLoggerFactory
        {
            private readonly IInitLogger _logger;

            public DummyInitLoggerFactory(IInitLogger logger)
            {
                _logger = logger;
            }

            public IInitLogger Create<T>()
            {
                return _logger;
            }
        }

        private class ThrowingTelemetryService : ITelemetryService
        {
            public Task AddActivityAsync(string activityName)
            {
                throw new InvalidOperationException("Test exception");
            }
        }
    }
}

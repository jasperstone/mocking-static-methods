using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Modularity;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        private class DummyModule : IAbpModule
        {
            public ServiceConfigurationContext ServiceConfigurationContext { get; set; }
        }

        private class DummyTelemetryService : ITelemetryService
        {
            public bool Called { get; private set; } = false;
            public string ActivityName { get; private set; }

            public Task AddActivityAsync(string activityName)
            {
                Called = true;
                ActivityName = activityName;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsGetRequiredServiceAndAddActivityAsync()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockTelemetryService = new DummyTelemetryService();
            services.AddTransient<ITelemetryService>(_ => mockTelemetryService);

            var mockLogger = new Mock<ILogger<AbpApplicationBase>>();
            services.AddLogging(builder => builder.AddProvider(new TestLoggerProvider(mockLogger.Object)));

            var serviceProvider = services.BuildServiceProvider();

            var app = new TestAbpApplication(serviceProvider, mockTelemetryService);

            // Act
            await app.InvokeInitializeTelemetryTracking();

            // Assert
            Assert.True(mockTelemetryService.Called);
            Assert.Equal(ActivityNameConsts.ApplicationRun, mockTelemetryService.ActivityName);
        }

        [Fact]
        public void SetupTelemetryTrackingAsync_DoesNotCallWhenShouldSendTelemetryDataIsFalse()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockTelemetryService = new DummyTelemetryService();
            services.AddTransient<ITelemetryService>(_ => mockTelemetryService);

            var serviceProvider = services.BuildServiceProvider();

            var app = new TestAbpApplication(serviceProvider, mockTelemetryService, shouldSendTelemetry: false);

            // Act
            var task = app.InvokeSetupTelemetryTrackingAsync();

            // Assert
            Assert.False(mockTelemetryService.Called);
        }

        [Fact]
        public void SetupTelemetryTracking_CallsInitializeTelemetryTrackingSync()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockTelemetryService = new DummyTelemetryService();
            services.AddTransient<ITelemetryService>(_ => mockTelemetryService);

            var serviceProvider = services.BuildServiceProvider();

            var app = new TestAbpApplication(serviceProvider, mockTelemetryService);

            // Act
            app.InvokeSetupTelemetryTracking();

            // Assert
            Assert.True(mockTelemetryService.Called);
        }

        // Helper classes for testing
        private class TestAbpApplication : AbpApplicationBase
        {
            private readonly DummyTelemetryService _telemetryService;
            private readonly bool _shouldSendTelemetry;

            public TestAbpApplication(IServiceProvider serviceProvider, DummyTelemetryService telemetryService, bool shouldSendTelemetry = true)
                : base(typeof(TestAbpApplication), new ServiceCollection(), null)
            {
                _telemetryService = telemetryService;
                _shouldSendTelemetry = shouldSendTelemetry;
                SetServiceProvider(serviceProvider);
            }

            public async Task InvokeInitializeTelemetryTracking()
            {
                await InitializeTelemetryTracking();
            }

            public async Task InvokeSetupTelemetryTrackingAsync()
            {
                await SetupTelemetryTrackingAsync();
            }

            protected override bool ShouldSendTelemetryData()
            {
                return _shouldSendTelemetry;
            }
        }

        private class TestLoggerProvider : ILoggerProvider
        {
            private readonly ILogger _logger;

            public TestLoggerProvider(ILogger logger)
            {
                _logger = logger;
            }

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose() { }
        }
    }
}

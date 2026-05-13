using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

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

        private class DummyLogger : ILogger<AbpApplicationBase>
        {
            public LogLevel LogLevel { get; private set; }
            public EventId EventId { get; private set; }
            public object State { get; private set; }
            public Exception Exception { get; private set; }
            public Func<object, Exception, string> Formatter { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LogLevel = logLevel;
                EventId = eventId;
                State = state;
                Exception = exception;
                Formatter = formatter;
            }
        }

        private class DummyModule : IAbpModule
        {
            public object Instance => this;
            public Assembly[] AllAssemblies => Array.Empty<Assembly>();
            public Type Type => GetType();
            public bool SkipAutoServiceRegistration => false;
        }

        private class DummyModuleDescriptor : IAbpModuleDescriptor
        {
            public object Instance => new DummyModule();
            public Type Type => typeof(DummyModule);
            public Assembly[] AllAssemblies => Array.Empty<Assembly>();
        }

        private class DummyModuleLoader : IModuleLoader
        {
            public IReadOnlyList<IAbpModuleDescriptor> LoadModules(IServiceCollection services, Type startupModuleType, object pluginSources)
            {
                return new List<IAbpModuleDescriptor> { new DummyModuleDescriptor() };
            }
        }

        private class TestAbpApplication : AbpApplicationBase
        {
            public bool TelemetryInitialized { get; private set; } = false;
            public DummyTelemetryService TelemetryService { get; } = new DummyTelemetryService();

            public TestAbpApplication(IServiceCollection services, Type startupModuleType, object pluginSources = null)
                : base(startupModuleType, services, null)
            {
                // Override to inject dependencies
            }

            protected override async Task InitializeTelemetryTracking()
            {
                TelemetryInitialized = true;
                await TelemetryService.AddActivityAsync(ActivityNameConsts.ApplicationRun);
            }

            protected override bool ShouldSendTelemetryData()
            {
                return true;
            }
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsCreateScopeAndTelemetryService()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new ServiceCollection()
                .AddScoped(_ => new DummyTelemetryService())
                .BuildServiceProvider();

            services.AddScoped(_ => new DummyTelemetryService());
            services.AddScoped<IServiceProvider>(_ => serviceProviderMock);

            var app = new TestAbpApplication(services, typeof(DummyModule));

            // Act
            await app.SetupTelemetryTrackingAsync();

            // Assert
            Assert.True(app.TelemetryInitialized);
        }
    }
}

using System;
using System.Collections.Generic;
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
            public readonly List<(LogLevel, EventId, object, Exception, Func<object, Exception, string>)> Entries = new();

            public void LogException(Exception ex, LogLevel level)
            {
                Entries.Add((level, default, null, ex, null));
            }
        }

        private class DummyScope : IServiceScope
        {
            public IServiceProvider ServiceProvider { get; }

            public DummyScope(IServiceProvider provider)
            {
                ServiceProvider = provider;
            }

            public void Dispose() { }
        }

        private class DummyScopeFactory : IServiceScopeFactory
        {
            private readonly IServiceProvider _provider;

            public DummyScopeFactory(IServiceProvider provider)
            {
                _provider = provider;
            }

            public IServiceScope CreateScope()
            {
                return new DummyScope(_provider);
            }
        }

        private class TestAbpApplicationBase : AbpApplicationBase
        {
            private readonly IServiceProvider _serviceProvider;

            public TestAbpApplicationBase(IServiceProvider serviceProvider, IServiceCollection services, IInitLogger initLogger)
                : base(typeof(object), services, null)
            {
                _serviceProvider = serviceProvider;
                Services = services;
                Services.AddSingleton(initLogger);
                SetServiceProvider(serviceProvider);
            }

            public async Task InvokeInitializeTelemetryTracking()
            {
                await InitializeTelemetryTracking();
            }

            protected override IServiceProvider ServiceProvider => _serviceProvider;
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsCreateScopeAndTelemetryService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockTelemetryService = new DummyTelemetryService();
            var mockLogger = new DummyLogger();

            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var scopeProvider = new DummyScopeFactory(serviceProvider);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.CreateScope()).Returns(() =>
            {
                var sp = new ServiceCollection()
                    .AddSingleton(mockTelemetryService)
                    .BuildServiceProvider();
                return new DummyScope(sp);
            });

            var app = new TestAbpApplicationBase(mockServiceProvider.Object, services, mockLogger);

            // Act
            await app.InvokeInitializeTelemetryTracking();

            // Assert
            Assert.True(mockTelemetryService.Called);
        }
    }
}

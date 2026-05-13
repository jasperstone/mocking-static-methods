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
            public LogLevel LogLevel { get; set; }
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                // do nothing
            }
        }

        private class DummyModule : IAbpModule
        {
            public ServiceConfigurationContext ServiceConfigurationContext { get; set; }
        }

        private class DummyModuleDescriptor : IAbpModuleDescriptor
        {
            public Type Type { get; set; }
            public IAbpModule Instance { get; set; }
            public IEnumerable<Assembly> AllAssemblies { get; set; }
        }

        private class DummyModuleLoader : IModuleLoader
        {
            public IReadOnlyList<IAbpModuleDescriptor> LoadModules(IServiceCollection services, Type startupModuleType, object pluginSources)
            {
                return new List<IAbpModuleDescriptor>();
            }
        }

        private class DummyAbpApplication : AbpApplicationBase
        {
            public DummyAbpApplication(IServiceCollection services) : base(typeof(DummyModule).Assembly.GetType(), services, null)
            {
            }
        }

        [Fact]
        public void CreateScope_CallsCreateScope()
        {
            var services = new ServiceCollection();
            var mockScope = new Mock<IServiceScope>();
            var mockScopeProvider = new Mock<IServiceProvider>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockModuleManager = new Mock<IModuleManager>();
            var mockTelemetryService = new DummyTelemetryService();

            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IModuleManager>()).Returns(mockModuleManager.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<ITelemetryService>()).Returns(mockTelemetryService);
            mockServiceProvider.Setup(sp => sp.CreateScope()).Returns(mockScope.Object);

            var app = new DummyAbpApplication(services);
            app.SetServiceProvider(mockServiceProvider.Object);

            // Act
            var task = app.GetType().GetMethod("InitializeTelemetryTracking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(app, null);
            // Wait for async method
            var taskResult = task as Task;
            taskResult?.GetAwaiter().GetResult();

            // Assert
            Assert.True(mockTelemetryService.Called);
        }
    }
}

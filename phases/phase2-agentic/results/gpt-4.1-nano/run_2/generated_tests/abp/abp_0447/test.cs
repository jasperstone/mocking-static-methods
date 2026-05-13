using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Logging;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        private class TestAbpApplication : AbpApplicationBase
        {
            public TestAbpApplication(Type startupModuleType, IServiceCollection services)
                : base(startupModuleType, services, null)
            {
            }

            public Task CallInitializeTelemetryTrackingAsync()
            {
                return InitializeTelemetryTracking();
            }
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsAddActivityAsync()
        {
            // Arrange
            var services = new ServiceCollection();

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock
                .Setup(t => t.AddActivityAsync(ActivityNameConsts.ApplicationRun))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ITelemetryService>())
                .Returns(telemetryServiceMock.Object);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock
                .Setup(f => f.CreateScope())
                .Returns(scopeMock.Object);

            services.AddSingleton(scopeFactoryMock.Object);
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);
            services.AddSingleton<IServiceScopeFactory>(scopeFactoryMock.Object);
            services.AddSingleton<ILogger<AbpApplicationBase>>(Mock.Of<ILogger<AbpApplicationBase>>());
            services.AddSingleton<IInitLoggerFactory>(new DummyInitLoggerFactory());

            var app = new TestAbpApplication(typeof(object), services);
            app.SetServiceProvider(serviceProviderMock.Object);

            // Act
            await app.CallInitializeTelemetryTrackingAsync();

            // Assert
            telemetryServiceMock.Verify(t => t.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        private class DummyInitLoggerFactory : IInitLoggerFactory
        {
            public IInitLogger Create<T>()
            {
                return new DummyInitLogger();
            }
        }

        private class DummyInitLogger : IInitLogger
        {
            public List<InitLoggerEntry> Entries { get; } = new List<InitLoggerEntry>();

            public void LogException(Exception ex, LogLevel level)
            {
                Entries.Add(new InitLoggerEntry
                {
                    LogLevel = level,
                    State = ex,
                    Exception = ex,
                    EventId = new EventId(0),
                    Formatter = (state, exception) => exception.Message
                });
            }
        }

        private class InitLoggerEntry
        {
            public LogLevel LogLevel { get; set; }
            public object State { get; set; }
            public Exception Exception { get; set; }
            public EventId EventId { get; set; }
            public Func<object, Exception, string> Formatter { get; set; }
        }
    }
}

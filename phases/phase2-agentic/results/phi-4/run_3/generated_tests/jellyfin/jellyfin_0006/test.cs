using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using System;
using System.Collections.Generic;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_LogsErrorOnDILoop()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<IPluginManager>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var startupOptionsMock = new Mock<IStartupOptions>();
            var startupConfigMock = new Mock<IConfiguration>();

            var applicationHost = new ApplicationHost(
                applicationPathsMock.Object,
                new LoggerFactory().AddProvider(new TestLoggerProvider()),
                startupOptionsMock.Object,
                startupConfigMock.Object)
            {
                Logger = loggerMock.Object,
                _pluginManager = pluginManagerMock.Object
            };

            var type = typeof(object);
            applicationHost._creatingInstances = new List<Type> { type };

            // Act
            var exception = Assert.Throws<TypeLoadException>(() => applicationHost.CreateInstanceSafe(type));

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("DI Loop detected in the attempted creation of")),
                    It.Is<object>(o => o == type.FullName)),
                Times.Once);

            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("Called from:")),
                    It.IsAny<object>()),
                Times.AtLeastOnce);

            pluginManagerMock.Verify(
                p => p.FailPlugin(type.Assembly),
                Times.Once);

            Assert.Equal("DI Loop detected", exception.Message);
        }
    }

    // Mock implementations for interfaces used in ApplicationHost
    public interface IPluginManager
    {
        void FailPlugin(Assembly assembly);
    }

    public interface IServerApplicationPaths
    {
        string VirtualDataPath { get; }
        string DataPath { get; }
        string VirtualInternalMetadataPath { get; }
        string InternalMetadataPath { get; }
    }

    public interface IConfiguration { }

    // Test logger provider for xUnit
    public class TestLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new TestLogger();

        public void Dispose() { }

        private class TestLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
        }
    }
}

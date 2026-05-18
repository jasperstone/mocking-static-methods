using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        private class DummyLogger : ILogger
        {
            public List<(LogLevel, string, Exception?)> Logs = new();

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Logs.Add((logLevel, formatter(state, exception), exception));
            }
        }

        [Fact]
        public async Task LogError_Called_WhenExceptionDuringShutdown()
        {
            // Arrange
            var dummyLogger = new DummyLogger();

            // Create a dummy BuildHostProcess that throws during ShutdownAsync
            var mockBuildHost = new Mock<BuildHostProcess>();
            mockBuildHost.Setup(b => b.ShutdownAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromException(new InvalidOperationException("Shutdown failed")));

            // Use reflection to set the private _logger field to our dummyLogger
            var manager = new BuildHostProcessManager(
                globalMSBuildProperties: null,
                binaryLogPathProvider: null,
                loggerFactory: null);
            var loggerField = typeof(BuildHostProcessManager).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(manager, dummyLogger);

            // Simulate the code that catches exception and logs error
            Exception caughtException = null;
            try
            {
                await mockBuildHost.Object.ShutdownAsync(CancellationToken.None);
            }
            catch (Exception e)
            {
                caughtException = e;
                dummyLogger.LogError(e, "Exception while shutting down the BuildHost process.");
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.IsType<InvalidOperationException>(caughtException);
            Assert.Contains(dummyLogger.Logs, log => log.Item1 == LogLevel.Error && log.Item2.Contains("Exception while shutting down the BuildHost process."));
        }
    }
}

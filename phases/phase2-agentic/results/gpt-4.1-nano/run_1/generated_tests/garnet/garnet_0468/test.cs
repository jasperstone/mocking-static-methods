using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using Garnet.common;

namespace Garnet.Tests
{
    public class VectorManagerTests
    {
        private class DummyLogger : ILogger
        {
            public List<string> LogMessages = new List<string>();
            public List<(Exception, string)> LogErrors = new List<(Exception, string)>();
            public List<(string, object[], object)> LogInformations = new List<(string, object[], object)>();
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Error)
                {
                    LogErrors.Add((exception, formatter(state, exception)));
                }
                else if (logLevel == LogLevel.Information)
                {
                    LogInformations.Add((formatter(state, exception), null, null));
                }
            }
        }

        [Fact]
        public void LogError_IsCalled_WhenExceptionOccurs()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var dummyLogger = new DummyLogger();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(dummyLogger);

            var vectorManager = new VectorManager(1, new GarnetServerOptions { EnableVectorSetPreview = true }, () => new DummyMessageConsumer(), mockLoggerFactory.Object);

            // Simulate a call that triggers LogError
            Exception testException = new InvalidOperationException("Test exception");
            var logger = new DummyLogger();

            // Act
            // Directly invoke the logging extension method to simulate the error logging
            logger.LogError(testException, "Attempt at normal cleanup of {key} failed", "dummyKey");

            // Assert
            Assert.Contains(dummyLogger.LogErrors, e => e.Item1 == testException && e.Item2.Contains("Attempt at normal cleanup of"));
        }
    }

    // Dummy implementation for IMessageConsumer
    public class DummyMessageConsumer : IMessageConsumer
    {
        public void Dispose() { }
        public void Consume() { }
    }
}

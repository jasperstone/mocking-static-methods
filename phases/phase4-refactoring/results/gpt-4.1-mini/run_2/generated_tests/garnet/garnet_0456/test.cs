using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Xunit;
using Garnet.server;
using Garnet.common;

namespace Garnet.Tests
{
    // Minimal test implementation of IRespCommandData for testing
    public class TestRespCommandData : IRespCommandData<TestRespCommandData>
    {
        public RespCommand Command { get; init; }
        public string Name { get; init; }
        public TestRespCommandData[] SubCommands { get; set; }
        public TestRespCommandData Parent { get; set; }
    }

    // Minimal mock IStreamProvider for testing
    public class MockStreamProvider : IStreamProvider
    {
        private readonly Stream _streamToReturn;
        public MockStreamProvider(Stream stream)
        {
            _streamToReturn = stream;
        }
        public Stream Read(string path)
        {
            return _streamToReturn;
        }
        public void Write(string path, byte[] data)
        {
            throw new NotImplementedException();
        }
    }

    public class RespCommandsDataProviderTests
    {
        [Fact]
        public void TryImportRespCommandsData_WhenJsonException_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var invalidJson = "invalid json";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
            var streamProvider = new MockStreamProvider(stream);

            var loggerCalled = false;
            Exception loggedException = null;
            string loggedMessage = null;
            string loggedPath = null;

            var logger = new TestLogger((ex, msg, path) =>
            {
                loggerCalled = true;
                loggedException = ex;
                loggedMessage = msg;
                loggedPath = path;
            });

            var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<TestRespCommandData>();

            // Act
            var result = provider.TryImportRespCommandsData("testpath", streamProvider, out var commandsData, logger);

            // Assert
            Assert.False(result);
            Assert.Null(commandsData);
            Assert.True(loggerCalled);
            Assert.IsType<System.Text.Json.JsonException>(loggedException);
            Assert.Equal("An error occurred while parsing resp command data file (Path: {path}).", loggedMessage);
            Assert.Equal("testpath", loggedPath);
        }

        // Helper logger to capture LogError calls
        private class TestLogger : ILogger
        {
            private readonly Action<Exception, string, string> _onLogError;

            public TestLogger(Action<Exception, string, string> onLogError)
            {
                _onLogError = onLogError;
            }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Error && exception != null)
                {
                    string path = null;
                    if (state is IReadOnlyList<KeyValuePair<string, object>> props)
                    {
                        foreach (var kvp in props)
                        {
                            if (kvp.Key == "path" && kvp.Value is string s)
                            {
                                path = s;
                                break;
                            }
                        }
                    }
                    _onLogError(exception, "An error occurred while parsing resp command data file (Path: {path}).", path);
                }
            }
        }
    }
}

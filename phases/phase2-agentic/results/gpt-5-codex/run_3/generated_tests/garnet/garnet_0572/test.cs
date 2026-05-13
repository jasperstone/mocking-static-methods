using System;
using System.Collections.Generic;
using System.IO;
using Garnet.server.TLS;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Garnet.server.TLS.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void Constructor_LogsErrorWhenCertificateCannotBeLoadedFromFile()
        {
            var logger = new TestLogger();
            var missingFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.pfx");

            var selector = new ServerCertificateSelector(missingFilePath, "password", logger: logger);

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Error, entry.LogLevel);
            Assert.NotNull(entry.Exception);
            Assert.Equal(
                "Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword.",
                entry.Message);

            Assert.Null(selector.GetSslServerCertificate());
        }

        private sealed class TestLogger : ILogger
        {
            public List<(LogLevel LogLevel, EventId EventId, object State, Exception Exception, string Message)> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString();
                Entries.Add((logLevel, eventId, state, exception, message ?? string.Empty));
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new NullScope();

                public void Dispose()
                {
                }
            }
        }
    }
}

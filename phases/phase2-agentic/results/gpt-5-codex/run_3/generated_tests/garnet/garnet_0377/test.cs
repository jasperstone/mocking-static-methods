using System;
using System.Collections.Generic;
using Garnet.server.Auth;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Garnet.Tests.Server.Auth
{
    public class GarnetAadAuthenticatorTests
    {
        [Fact]
        public void Authenticate_WhenExceptionOccurs_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var logger = new TestLogger();
            var authenticator = new GarnetAadAuthenticator(
                authorizedAppIds: new[] { "app1" },
                audiences: new[] { "audience1" },
                issuers: new[] { "issuer1" },
                signingTokenProvider: null!,
                validateUsername: false,
                logger: logger);

            // Act
            var result = authenticator.Authenticate(ReadOnlySpan<byte>.Empty, ReadOnlySpan<byte>.Empty);

            // Assert
            Assert.False(result);
            Assert.False(authenticator.IsAuthenticated);

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Error, entry.Level);
            Assert.NotNull(entry.Exception);
            Assert.Equal("Authentication failed", entry.Message);
        }

        private sealed class TestLogger : ILogger
        {
            private readonly List<LogEntry> _entries = new();

            public IReadOnlyList<LogEntry> Entries => _entries;

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
                _entries.Add(new LogEntry(logLevel, exception, message));
            }

            public sealed record LogEntry(LogLevel Level, Exception Exception, string Message);

            private sealed class NullScope : IDisposable
            {
                public static NullScope Instance { get; } = new NullScope();

                private NullScope()
                {
                }

                public void Dispose()
                {
                }
            }
        }
    }
}

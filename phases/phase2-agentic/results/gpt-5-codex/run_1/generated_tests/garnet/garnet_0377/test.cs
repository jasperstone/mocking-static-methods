using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Garnet.server.Auth;
using Garnet.server.Auth.Aad;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Garnet.Tests.Auth
{
    public class GarnetAadAuthenticatorTests
    {
        [Fact]
        public void Authenticate_WhenTokenValidationFails_LogsErrorAndReturnsFalse()
        {
            var logger = new TestLogger();
            var signingTokens = new List<SecurityKey>
            {
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes("01234567890123456789012345678901"))
            };
            using var signingTokenProvider = new TestIssuerSigningTokenProvider(signingTokens);

            var authenticator = new GarnetAadAuthenticator(
                authorizedAppIds: new[] { "app1" },
                audiences: new[] { "aud1" },
                issuers: new[] { "iss1" },
                signingTokenProvider: signingTokenProvider,
                validateUsername: false,
                logger: logger);

            var result = authenticator.Authenticate(
                Encoding.UTF8.GetBytes("invalid-token"),
                Encoding.UTF8.GetBytes("user"));

            Assert.False(result);

            var errorEntry = Assert.Single(logger.Entries.Where(entry => entry.Level == LogLevel.Error));
            Assert.Equal("Authentication failed", errorEntry.Message);
            Assert.NotNull(errorEntry.Exception);
        }

        private sealed class TestLogger : ILogger
        {
            public List<(LogLevel Level, string Message, Exception Exception)> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
                Entries.Add((logLevel, message, exception));
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new NullScope();
                public void Dispose()
                {
                }
            }
        }

        private sealed class TestIssuerSigningTokenProvider : IssuerSigningTokenProvider
        {
            public TestIssuerSigningTokenProvider(IReadOnlyCollection<SecurityKey> signingTokens)
                : base("https://authority.example.com", signingTokens, refreshTokens: false, logger: null)
            {
            }
        }
    }
}

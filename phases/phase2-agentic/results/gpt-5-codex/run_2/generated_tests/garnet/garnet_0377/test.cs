using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Garnet.Server.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Garnet.Server.Auth.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        [Fact]
        public void Authenticate_WhenSigningTokenProviderThrows_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["GarnetAuthentication:ValidAudiences:0"] = "audience1",
                    ["GarnetAuthentication:ValidIssuers:0"] = "issuer1",
                    ["GarnetAuthentication:ValidAppIds:0"] = "app1",
                    ["GarnetAuthentication:ValidOidClaim"] = "oid",
                    ["GarnetAuthentication:ValidGroupClaim"] = "group",
                    ["GarnetAuthentication:ValidAppIdClaim"] = "appId",
                    ["GarnetAuthentication:ValidateUsername"] = "false",
                })
                .Build();

            var signingTokenProvider = new ThrowingSigningTokenProvider();
            var scopeValidator = new AlwaysPassScopeValidator();
            var logger = new TestLogger<GarnetAadAuthenticator>();

            var authenticator = new GarnetAadAuthenticator(signingTokenProvider, scopeValidator, configuration, logger);

            var password = Encoding.UTF8.GetBytes("token");
            var username = Encoding.UTF8.GetBytes("user");

            // Act
            var result = authenticator.Authenticate(password, username);

            // Assert
            Assert.False(result);

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Error, entry.LogLevel);
            Assert.Equal("Authentication failed", entry.Message);
            Assert.Same(signingTokenProvider.ThrownException, entry.Exception);
        }

        private sealed class ThrowingSigningTokenProvider : ISigningTokenProvider
        {
            public Exception ThrownException { get; } = new InvalidOperationException("Unable to retrieve signing tokens.");

            public IEnumerable<SecurityKey> SigningTokens => throw ThrownException;
        }

        private sealed class AlwaysPassScopeValidator : IScopeValidator
        {
            public bool TryValidateScope(ClaimsPrincipal claimsPrincipal, string[] validAppIds) => true;
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
                Entries.Add(new LogEntry(logLevel, message, exception));
            }
        }

        private sealed class LogEntry
        {
            public LogEntry(LogLevel logLevel, string message, Exception? exception)
            {
                LogLevel = logLevel;
                Message = message;
                Exception = exception;
            }

            public LogLevel LogLevel { get; }

            public string Message { get; }

            public Exception? Exception { get; }
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();

            public void Dispose()
            {
            }
        }
    }
}

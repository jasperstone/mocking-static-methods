using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.Auth;

namespace GarnetTests
{
    public class GarnetAadAuthenticatorTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IssuerSigningTokenProvider> _signingTokenProviderMock;
        private readonly IReadOnlyCollection<string> _authorizedAppIds = new[] { "appId1" };
        private readonly IReadOnlyCollection<string> _audiences = new[] { "audience1" };
        private readonly IReadOnlyCollection<string> _issuers = new[] { "issuer1" };

        public GarnetAadAuthenticatorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _signingTokenProviderMock = new Mock<IssuerSigningTokenProvider>();
            _signingTokenProviderMock.Setup(p => p.SigningTokens).Returns(new List<Microsoft.IdentityModel.Tokens.SecurityKey>());
        }

        [Fact]
        public void Authenticate_WhenTokenValidationThrows_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var authenticator = new TestGarnetAadAuthenticator(
                _authorizedAppIds,
                _audiences,
                _issuers,
                _signingTokenProviderMock.Object,
                validateUsername: false,
                _loggerMock.Object);

            var password = Encoding.UTF8.GetBytes("invalid_token");
            var username = Encoding.UTF8.GetBytes("user");

            // Act
            var result = authenticator.Authenticate(password, username);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Authentication failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper class to override ValidateToken to throw exception to test error logging
        private class TestGarnetAadAuthenticator : GarnetAadAuthenticator
        {
            public TestGarnetAadAuthenticator(
                IReadOnlyCollection<string> authorizedAppIds,
                IReadOnlyCollection<string> audiences,
                IReadOnlyCollection<string> issuers,
                IssuerSigningTokenProvider signingTokenProvider,
                bool validateUsername,
                ILogger logger)
                : base(authorizedAppIds, audiences, issuers, signingTokenProvider, validateUsername, logger)
            {
            }

            // Override the static token handler to throw exception to simulate failure
            public override bool Authenticate(ReadOnlySpan<byte> password, ReadOnlySpan<byte> username)
            {
                throw new Exception("Simulated failure");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.Auth;

namespace Garnet.Tests.Auth
{
    public class GarnetAadAuthenticatorTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IssuerSigningTokenProvider> _signingTokenProviderMock;
        private readonly IReadOnlyCollection<string> _authorizedAppIds = new List<string> { "appId1" };
        private readonly IReadOnlyCollection<string> _audiences = new List<string> { "aud1" };
        private readonly IReadOnlyCollection<string> _issuers = new List<string> { "issuer1" };

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
            var authenticator = new GarnetAadAuthenticator(
                _authorizedAppIds,
                _audiences,
                _issuers,
                _signingTokenProviderMock.Object,
                validateUsername: false,
                _loggerMock.Object);

            // Provide invalid token bytes that will cause ValidateToken to throw
            var invalidTokenBytes = Encoding.UTF8.GetBytes("invalid_token");
            var usernameBytes = Encoding.UTF8.GetBytes("username");

            // Act
            var result = authenticator.Authenticate(invalidTokenBytes, usernameBytes);

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
    }
}

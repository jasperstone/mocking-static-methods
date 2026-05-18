using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.Auth;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace GarnetTests
{
    public class GarnetAadAuthenticatorTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IssuerSigningTokenProvider> _signingTokenProviderMock;
        private readonly List<string> _authorizedAppIds;
        private readonly List<string> _audiences;
        private readonly List<string> _issuers;

        public GarnetAadAuthenticatorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _signingTokenProviderMock = new Mock<IssuerSigningTokenProvider>("https://authority", null, false, null);
            _authorizedAppIds = new List<string> { "appId1" };
            _audiences = new List<string> { "aud1" };
            _issuers = new List<string> { "issuer1" };
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

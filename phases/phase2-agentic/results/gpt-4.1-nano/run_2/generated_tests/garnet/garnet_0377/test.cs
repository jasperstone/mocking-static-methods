using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IssuerSigningTokenProvider> _tokenProviderMock;
        private readonly List<string> _authorizedAppIds = new List<string> { "app1" };
        private readonly List<string> _audiences = new List<string> { "aud1" };
        private readonly List<string> _issuers = new List<string> { "issuer1" };
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();

        public GarnetAadAuthenticatorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _tokenProviderMock = new Mock<IssuerSigningTokenProvider>();
            _tokenProviderMock.Setup(tp => tp.SigningTokens).Returns(new List<SecurityKey>());
        }

        [Fact]
        public void Authenticate_Should_LogError_When_ExceptionThrown()
        {
            // Arrange
            var authenticator = new Garnet.server.Auth.GarnetAadAuthenticator(
                _authorizedAppIds,
                _audiences,
                _issuers,
                _tokenProviderMock.Object,
                validateUsername: false,
                logger: _loggerMock.Object);

            var invalidPassword = new ReadOnlySpan<byte>(new byte[] { 0x00 });
            var username = new ReadOnlySpan<byte>(new byte[] { 0x00 });

            // Act
            var result = authenticator.Authenticate(invalidPassword, username);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Authentication failed"),
                Times.Once);
        }
    }
}

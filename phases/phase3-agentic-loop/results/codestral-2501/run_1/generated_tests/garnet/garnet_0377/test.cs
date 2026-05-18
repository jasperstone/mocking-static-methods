using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Garnet.server.Auth;
using Garnet.server.Auth.Aad;
using Microsoft.IdentityModel.Tokens;

namespace Garnet.server.Auth.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IssuerSigningTokenProvider> _signingTokenProviderMock;
        private readonly List<string> _authorizedAppIds;
        private readonly List<string> _audiences;
        private readonly List<string> _issuers;
        private readonly bool _validateUsername;

        public GarnetAadAuthenticatorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _signingTokenProviderMock = new Mock<IssuerSigningTokenProvider>();
            _authorizedAppIds = new List<string> { "app1", "app2" };
            _audiences = new List<string> { "audience1", "audience2" };
            _issuers = new List<string> { "issuer1", "issuer2" };
            _validateUsername = true;
        }

        [Fact]
        public void Authenticate_ValidToken_ReturnsTrue()
        {
            // Arrange
            var tokenHandlerMock = new Mock<JwtSecurityTokenHandler>();
            var claims = new List<Claim>
            {
                new Claim("appid", "app1"),
                new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "user1")
            };
            var identity = new ClaimsPrincipal(new ClaimsIdentity(claims));
            var token = new JwtSecurityToken(claims: claims);
            tokenHandlerMock.Setup(th => th.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out It.Ref<SecurityToken>.IsAny)).Returns(identity);

            var authenticator = new GarnetAadAuthenticator(
                _authorizedAppIds,
                _audiences,
                _issuers,
                _signingTokenProviderMock.Object,
                _validateUsername,
                _loggerMock.Object);

            // Act
            var result = authenticator.Authenticate(Encoding.UTF8.GetBytes("password"), Encoding.UTF8.GetBytes("user1"));

            // Assert
            Assert.True(result);
            _loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void Authenticate_InvalidToken_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var tokenHandlerMock = new Mock<JwtSecurityTokenHandler>();
            tokenHandlerMock.Setup(th => th.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out It.Ref<SecurityToken>.IsAny)).Throws(new SecurityTokenException("Invalid token"));

            var authenticator = new GarnetAadAuthenticator(
                _authorizedAppIds,
                _audiences,
                _issuers,
                _signingTokenProviderMock.Object,
                _validateUsername,
                _loggerMock.Object);

            // Act
            var result = authenticator.Authenticate(Encoding.UTF8.GetBytes("password"), Encoding.UTF8.GetBytes("user1"));

            // Assert
            Assert.False(result);
            _loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}

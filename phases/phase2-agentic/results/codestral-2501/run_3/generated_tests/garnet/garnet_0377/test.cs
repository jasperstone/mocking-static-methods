using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Garnet.server.Auth;
using Garnet.server.Auth.Aad;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace Garnet.Tests.Auth
{
    public class GarnetAadAuthenticatorTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IssuerSigningTokenProvider> _signingTokenProviderMock;
        private readonly GarnetAadAuthenticator _authenticator;

        public GarnetAadAuthenticatorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _signingTokenProviderMock = new Mock<IssuerSigningTokenProvider>();
            _authenticator = new GarnetAadAuthenticator(
                new List<string> { "authorizedAppId" },
                new List<string> { "validAudience" },
                new List<string> { "validIssuer" },
                _signingTokenProviderMock.Object,
                true,
                _loggerMock.Object);
        }

        [Fact]
        public void Authenticate_ValidToken_ReturnsTrue()
        {
            // Arrange
            var tokenHandlerMock = new Mock<JwtSecurityTokenHandler>();
            var token = new JwtSecurityToken(
                issuer: "validIssuer",
                audience: "validAudience",
                claims: new[] { new Claim("appid", "authorizedAppId") },
                notBefore: DateTime.UtcNow.AddMinutes(-1),
                expires: DateTime.UtcNow.AddMinutes(1),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret")), SecurityAlgorithms.HmacSha256));

            tokenHandlerMock.Setup(t => t.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out token))
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim("appid", "authorizedAppId") })));

            var password = Encoding.UTF8.GetBytes("validPassword");
            var username = Encoding.UTF8.GetBytes("validUsername");

            // Act
            var result = _authenticator.Authenticate(password, username);

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
            var password = Encoding.UTF8.GetBytes("invalidPassword");
            var username = Encoding.UTF8.GetBytes("invalidUsername");

            // Act
            var result = _authenticator.Authenticate(password, username);

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

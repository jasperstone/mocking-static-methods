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
            _signingTokenProviderMock.Setup(x => x.SigningTokens).Returns(new List<SecurityKey>());

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
            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = new[]
            {
                new Claim("appid", "authorizedAppId"),
                new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "validUser")
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = "validIssuer",
                Audience = "validAudience",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret")), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Act
            var result = _authenticator.Authenticate(Encoding.UTF8.GetBytes(tokenString), Encoding.UTF8.GetBytes("validUser"));

            // Assert
            Assert.True(result);
            _loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void Authenticate_InvalidToken_ReturnsFalse()
        {
            // Arrange
            var invalidToken = "invalidToken";

            // Act
            var result = _authenticator.Authenticate(Encoding.UTF8.GetBytes(invalidToken), Encoding.UTF8.GetBytes("validUser"));

            // Assert
            Assert.False(result);
            _loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}

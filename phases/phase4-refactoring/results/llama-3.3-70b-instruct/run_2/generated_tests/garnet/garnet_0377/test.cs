using Xunit;
using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Garnet.server.Auth.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        [Fact]
        public void Authenticate_ValidToken_ReturnsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var authenticator = new GarnetAadAuthenticator(
                new[] { "authorizedAppId" },
                new[] { "audience" },
                new[] { "issuer" },
                new IssuerSigningTokenProvider(),
                false,
                loggerMock.Object);

            var token = new JwtSecurityToken(
                issuer: "issuer",
                audience: "audience",
                claims: new[] { new Claim("appid", "authorizedAppId") },
                expires: DateTime.UtcNow.AddHours(1));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            // Act
            var result = authenticator.Authenticate(Encoding.UTF8.GetBytes(tokenString), Encoding.UTF8.GetBytes("username"));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Authenticate_InvalidToken_ReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var authenticator = new GarnetAadAuthenticator(
                new[] { "authorizedAppId" },
                new[] { "audience" },
                new[] { "issuer" },
                new IssuerSigningTokenProvider(),
                false,
                loggerMock.Object);

            var token = new JwtSecurityToken(
                issuer: "invalidIssuer",
                audience: "audience",
                claims: new[] { new Claim("appid", "authorizedAppId") },
                expires: DateTime.UtcNow.AddHours(1));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            // Act
            var result = authenticator.Authenticate(Encoding.UTF8.GetBytes(tokenString), Encoding.UTF8.GetBytes("username"));

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Authenticate_ThrowsException_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var authenticator = new GarnetAadAuthenticator(
                new[] { "authorizedAppId" },
                new[] { "audience" },
                new[] { "issuer" },
                new IssuerSigningTokenProvider(),
                false,
                loggerMock.Object);

            var token = new JwtSecurityToken(
                issuer: "issuer",
                audience: "audience",
                claims: new[] { new Claim("appid", "authorizedAppId") },
                expires: DateTime.UtcNow.AddHours(1));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            // Act and Assert
            loggerMock.Setup(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()));
            authenticator.Authenticate(Encoding.UTF8.GetBytes(tokenString), Encoding.UTF8.GetBytes(null));
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }
}

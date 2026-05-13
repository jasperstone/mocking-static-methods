using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Garnet.server.Auth.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        [Fact]
        public void Authenticate_ValidToken_ReturnsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var signingTokenProvider = new IssuerSigningTokenProvider(new List<SecurityKey>());
            var authenticator = new GarnetAadAuthenticator(
                new List<string>(),
                new List<string>(),
                new List<string>(),
                signingTokenProvider,
                false,
                loggerMock.Object);

            var token = new JwtSecurityToken(
                issuer: "issuer",
                audience: "audience",
                claims: new Claim[] { new Claim("claim", "value") },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(new byte[32]), SecurityAlgorithms.HmacSha256));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            // Act
            var result = authenticator.Authenticate(Encoding.UTF8.GetBytes(tokenString), Encoding.UTF8.GetBytes("username"));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Authenticate_InvalidToken_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var signingTokenProvider = new IssuerSigningTokenProvider(new List<SecurityKey>());
            var authenticator = new GarnetAadAuthenticator(
                new List<string>(),
                new List<string>(),
                new List<string>(),
                signingTokenProvider,
                false,
                loggerMock.Object);

            // Act
            authenticator.Authenticate(Encoding.UTF8.GetBytes("invalid token"), Encoding.UTF8.GetBytes("username"));

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Authentication failed"), Times.Once);
        }
    }
}

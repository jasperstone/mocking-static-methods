using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Garnet.server.Auth.Aad;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Validators;

namespace Garnet.server.Auth.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IssuerSigningTokenProvider> _mockSigningTokenProvider;
        private readonly List<string> _authorizedAppIds;
        private readonly List<string> _audiences;
        private readonly List<string> _issuers;
        private readonly bool _validateUsername;

        public GarnetAadAuthenticatorTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockSigningTokenProvider = new Mock<IssuerSigningTokenProvider>();
            _authorizedAppIds = new List<string> { "app1", "app2" };
            _audiences = new List<string> { "audience1", "audience2" };
            _issuers = new List<string> { "issuer1", "issuer2" };
            _validateUsername = true;
        }

        [Fact]
        public void Authenticate_ValidToken_LogsInformation()
        {
            // Arrange
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySuperSecretKey"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("appid", "app1"),
                new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "user1")
            };
            var token = new JwtSecurityToken(
                issuer: "issuer1",
                audience: "audience1",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );
            var tokenString = tokenHandler.WriteToken(token);

            var authenticator = new GarnetAadAuthenticator(
                _authorizedAppIds,
                _audiences,
                _issuers,
                _mockSigningTokenProvider.Object,
                _validateUsername,
                _mockLogger.Object
            );

            // Act
            var result = authenticator.Authenticate(Encoding.UTF8.GetBytes(tokenString), Encoding.UTF8.GetBytes("user1"));

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.True(result);
        }

        [Fact]
        public void Authenticate_InvalidToken_LogsError()
        {
            // Arrange
            var authenticator = new GarnetAadAuthenticator(
                _authorizedAppIds,
                _audiences,
                _issuers,
                _mockSigningTokenProvider.Object,
                _validateUsername,
                _mockLogger.Object
            );

            // Act
            var result = authenticator.Authenticate(Encoding.UTF8.GetBytes("invalidToken"), Encoding.UTF8.GetBytes("user1"));

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result);
        }
    }
}

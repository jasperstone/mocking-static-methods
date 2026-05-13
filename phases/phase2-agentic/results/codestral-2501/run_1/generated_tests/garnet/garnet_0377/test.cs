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

namespace Garnet.server.Auth.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IssuerSigningTokenProvider> _mockSigningTokenProvider;
        private readonly List<string> _authorizedAppIds;
        private readonly List<string> _audiences;
        private readonly List<string> _issuers;
        private readonly GarnetAadAuthenticator _authenticator;

        public GarnetAadAuthenticatorTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockSigningTokenProvider = new Mock<IssuerSigningTokenProvider>();
            _authorizedAppIds = new List<string> { "app1", "app2" };
            _audiences = new List<string> { "audience1", "audience2" };
            _issuers = new List<string> { "issuer1", "issuer2" };

            _authenticator = new GarnetAadAuthenticator(
                _authorizedAppIds,
                _audiences,
                _issuers,
                _mockSigningTokenProvider.Object,
                true,
                _mockLogger.Object);
        }

        [Fact]
        public void Authenticate_ValidToken_ReturnsTrue()
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
                signingCredentials: creds);

            var tokenString = tokenHandler.WriteToken(token);
            var password = Encoding.UTF8.GetBytes(tokenString);
            var username = Encoding.UTF8.GetBytes("user1");

            _mockSigningTokenProvider.Setup(x => x.SigningTokens).Returns(new List<SecurityKey> { key });

            // Act
            var result = _authenticator.Authenticate(password, username);

            // Assert
            Assert.True(result);
            _mockLogger.Verify(x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Authenticate_InvalidToken_ReturnsFalse()
        {
            // Arrange
            var password = Encoding.UTF8.GetBytes("invalidToken");
            var username = Encoding.UTF8.GetBytes("user1");

            // Act
            var result = _authenticator.Authenticate(password, username);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }
}

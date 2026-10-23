using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Garnet.server.Auth;
using Garnet.server.Auth.Aad;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Validators;
using Moq;
using Xunit;

namespace Garnet.server.Auth.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        [Fact]
        public void Authenticate_ValidToken_ReturnsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var signingTokenProviderMock = new Mock<IssuerSigningTokenProvider>();
            var tokenHandlerMock = new Mock<JwtSecurityTokenHandler>();

            var authorizedAppIds = new List<string> { "app1" };
            var audiences = new List<string> { "audience1" };
            var issuers = new List<string> { "issuer1" };
            var validateUsername = false;

            var authenticator = new GarnetAadAuthenticator(
                authorizedAppIds,
                audiences,
                issuers,
                signingTokenProviderMock.Object,
                validateUsername,
                loggerMock.Object);

            var token = new JwtSecurityToken(
                issuer: "issuer1",
                audience: "audience1",
                claims: new[] { new Claim("appid", "app1") },
                notBefore: DateTime.UtcNow.AddMinutes(-1),
                expires: DateTime.UtcNow.AddMinutes(1));

            tokenHandlerMock.Setup(t => t.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out It.Ref<SecurityToken>.IsAny))
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim("appid", "app1") })));

            // Act
            var result = authenticator.Authenticate(Encoding.UTF8.GetBytes("password"), Encoding.UTF8.GetBytes("username"));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Authenticate_InvalidToken_ReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var signingTokenProviderMock = new Mock<IssuerSigningTokenProvider>();
            var tokenHandlerMock = new Mock<JwtSecurityTokenHandler>();

            var authorizedAppIds = new List<string> { "app1" };
            var audiences = new List<string> { "audience1" };
            var issuers = new List<string> { "issuer1" };
            var validateUsername = false;

            var authenticator = new GarnetAadAuthenticator(
                authorizedAppIds,
                audiences,
                issuers,
                signingTokenProviderMock.Object,
                validateUsername,
                loggerMock.Object);

            tokenHandlerMock.Setup(t => t.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out It.Ref<SecurityToken>.IsAny))
                .Throws(new SecurityTokenException("Invalid token"));

            // Act
            var result = authenticator.Authenticate(Encoding.UTF8.GetBytes("password"), Encoding.UTF8.GetBytes("username"));

            // Assert
            Assert.False(result);
        }
    }
}

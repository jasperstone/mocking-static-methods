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

namespace Garnet.server.Auth
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
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidIssuers = new List<string> { "issuer" },
                ValidAudiences = new List<string> { "audience" },
                IssuerSigningKeys = new List<SecurityKey> { new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret")) }
            };
            tokenValidationParameters.EnableAadSigningKeyIssuerValidation();
            var token = new JwtSecurityToken("issuer", "audience", new[] { new Claim("appid", "appid") }, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            tokenHandlerMock.Setup(th => th.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out It.Ref<SecurityToken>.IsAny)).Returns((ClaimsPrincipal)null);
            var authenticator = new GarnetAadAuthenticator(new List<string> { "appid" }, new List<string> { "audience" }, new List<string> { "issuer" }, signingTokenProviderMock.Object, false, loggerMock.Object);

            // Act
            var result = authenticator.Authenticate(Encoding.UTF8.GetBytes(token.RawData), Encoding.UTF8.GetBytes("username"));

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
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidIssuers = new List<string> { "issuer" },
                ValidAudiences = new List<string> { "audience" },
                IssuerSigningKeys = new List<SecurityKey> { new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret")) }
            };
            tokenValidationParameters.EnableAadSigningKeyIssuerValidation();
            var token = new JwtSecurityToken("issuer", "audience", new[] { new Claim("appid", "appid") }, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            tokenHandlerMock.Setup(th => th.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out It.Ref<SecurityToken>.IsAny)).Throws<SecurityTokenException>();
            var authenticator = new GarnetAadAuthenticator(new List<string> { "appid" }, new List<string> { "audience" }, new List<string> { "issuer" }, signingTokenProviderMock.Object, false, loggerMock.Object);

            // Act
            var result = authenticator.Authenticate(Encoding.UTF8.GetBytes(token.RawData), Encoding.UTF8.GetBytes("username"));

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Authenticate_LoggerErrorCalled_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var signingTokenProviderMock = new Mock<IssuerSigningTokenProvider>();
            var tokenHandlerMock = new Mock<JwtSecurityTokenHandler>();
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidIssuers = new List<string> { "issuer" },
                ValidAudiences = new List<string> { "audience" },
                IssuerSigningKeys = new List<SecurityKey> { new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret")) }
            };
            tokenValidationParameters.EnableAadSigningKeyIssuerValidation();
            var token = new JwtSecurityToken("issuer", "audience", new[] { new Claim("appid", "appid") }, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            tokenHandlerMock.Setup(th => th.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out It.Ref<SecurityToken>.IsAny)).Throws<Exception>();
            var authenticator = new GarnetAadAuthenticator(new List<string> { "appid" }, new List<string> { "audience" }, new List<string> { "issuer" }, signingTokenProviderMock.Object, false, loggerMock.Object);

            // Act
            authenticator.Authenticate(Encoding.UTF8.GetBytes(token.RawData), Encoding.UTF8.GetBytes("username"));

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Authentication failed"), Times.Once);
        }
    }
}

using Garnet.server.Auth;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace GarnetAuthTests
{
    public class GarnetAadAuthenticatorTests
    {
        [Fact]
        public void Authenticate_InvalidToken_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var authenticator = new GarnetAadAuthenticator(
                new[] { "appId1" },
                new[] { "audience1" },
                new[] { "issuer1" },
                new Mock<IssuerSigningTokenProvider>().Object,
                true,
                loggerMock.Object);

            // Act
            authenticator.Authenticate(Encoding.UTF8.GetBytes("invalid token"), Encoding.UTF8.GetBytes("username"));

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Authentication failed"), Times.Once);
        }

        [Fact]
        public void Authenticate_ValidToken_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "oid") }),
                Issuer = "issuer1",
                Audience = "audience1",
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret key")), SecurityAlgorithms.HmacSha256)
            });
            var authenticator = new GarnetAadAuthenticator(
                new[] { "appId1" },
                new[] { "audience1" },
                new[] { "issuer1" },
                new Mock<IssuerSigningTokenProvider>().Object,
                true,
                loggerMock.Object);

            // Act
            authenticator.Authenticate(Encoding.UTF8.GetBytes(tokenHandler.WriteToken(token)), Encoding.UTF8.GetBytes("oid"));

            // Assert
            loggerMock.Verify(l => l.LogInformation("Authentication successful. Token valid from {validFrom} to {validateTo}", It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        }
    }
}

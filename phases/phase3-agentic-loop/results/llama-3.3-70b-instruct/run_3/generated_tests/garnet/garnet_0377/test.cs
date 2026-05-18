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
        public void Authenticate_ThrowsException_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var authenticator = new Garnet.server.Auth.GarnetAadAuthenticator(
                new List<string>(),
                new List<string>(),
                new List<string>(),
                new Garnet.server.Auth.Aad.IssuerSigningTokenProvider(),
                false,
                loggerMock.Object);

            // Act and Assert
            authenticator.Authenticate(new ReadOnlySpan<byte>(), new ReadOnlySpan<byte>());
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Authentication failed"), Times.Once);
        }

        [Fact]
        public void Authenticate_ValidToken_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var tokenHandlerMock = new Mock<JwtSecurityTokenHandler>();
            tokenHandlerMock
                .Setup(th => th.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out It.Ref<SecurityToken>.IsAny))
                .Returns(new ClaimsPrincipal());
            var authenticator = new Garnet.server.Auth.GarnetAadAuthenticator(
                new List<string>(),
                new List<string>(),
                new List<string>(),
                new Garnet.server.Auth.Aad.IssuerSigningTokenProvider(),
                false,
                loggerMock.Object);

            // Act
            authenticator.Authenticate(new ReadOnlySpan<byte>(), new ReadOnlySpan<byte>());

            // Assert
            loggerMock.Verify(l => l.LogInformation("Authentication successful. Token valid from {validFrom} to {validateTo}", It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        }
    }
}

using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Garnet.server.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Garnet.server.Auth.Tests
{
    public class GarnetAadAuthenticatorTests
    {
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
            authenticator.Authenticate(new ReadOnlySpan<byte>(), new ReadOnlySpan<byte>());

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Authentication failed"), Times.Once);
        }

        [Fact]
        public void Authenticate_ValidToken_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var tokenHandlerMock = new Mock<JwtSecurityTokenHandler>();
            tokenHandlerMock.Setup(th => th.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out It.Ref<SecurityToken>.IsAny))
                .Returns(new ClaimsPrincipal());
            var signingTokenProvider = new IssuerSigningTokenProvider(new List<SecurityKey>());
            var authenticator = new GarnetAadAuthenticator(
                new List<string>(),
                new List<string>(),
                new List<string>(),
                signingTokenProvider,
                false,
                loggerMock.Object);

            // Act
            authenticator.Authenticate(new ReadOnlySpan<byte>(), new ReadOnlySpan<byte>());

            // Assert
            loggerMock.Verify(l => l.LogInformation("Authentication successful. Token valid from {validFrom} to {validateTo}", It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        }
    }
}

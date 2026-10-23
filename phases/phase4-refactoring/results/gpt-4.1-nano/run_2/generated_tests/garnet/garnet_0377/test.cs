using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Garnet.server.Auth;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        [Fact]
        public void Authenticate_Should_LogError_When_ValidateToken_Throws()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GarnetAadAuthenticator>>();
            var signingTokens = new List<Microsoft.IdentityModel.Tokens.SecurityKey>();
            var tokenProvider = new IssuerSigningTokenProvider(signingTokens);
            var authenticator = new GarnetAadAuthenticator(
                new List<string> { "app1" },
                new List<string> { "aud1" },
                new List<string> { "issuer1" },
                tokenProvider,
                validateUsername: false,
                logger: loggerMock.Object);

            // Force the JwtSecurityTokenHandler to throw during validation
            // by replacing the static _tokenHandler with a mock or by causing an exception
            // Since _tokenHandler is static, we simulate an exception by passing invalid token data

            var invalidTokenBytes = Encoding.UTF8.GetBytes("invalid_token");
            var result = false;

            // Act
            result = authenticator.Authenticate(invalidTokenBytes, Encoding.UTF8.GetBytes("user"));

            // Assert
            // Verify that LogError was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Authentication failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

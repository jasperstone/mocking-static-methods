using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.Auth;

namespace Garnet.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        [Fact]
        public void Authenticate_ShouldLogError_WhenExceptionThrown()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockSigningTokens = new List<SecurityKey> { new SymmetricSecurityKey(Encoding.UTF8.GetBytes("testkeytestkeytestkeytestkey")) };
            var tokenHandler = new JwtSecurityTokenHandler();

            var authenticator = new GarnetAadAuthenticator(
                authorizedAppIds: new List<string> { "app1" },
                audiences: new List<string> { "aud1" },
                issuers: new List<string> { "issuer1" },
                signingTokenProvider: new IssuerSigningTokenProvider(mockSigningTokens),
                validateUsername: false,
                logger: mockLogger.Object);

            // Use reflection or other means to replace the static _tokenHandler with one that throws
            // Since _tokenHandler is static, we can't directly mock it easily.
            // Instead, we can simulate an exception by passing invalid token data.

            var invalidTokenBytes = Encoding.UTF8.GetBytes("invalidtoken");

            // Act
            var result = authenticator.Authenticate(invalidTokenBytes, new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes("user1")));

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Authentication failed")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Security.Claims;
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
            var mockTokenHandler = new Mock<JwtSecurityTokenHandler>();
            var mockSigningTokenProvider = new Mock<IssuerSigningTokenProvider>();

            var authorizedAppIds = new List<string> { "app1" };
            var audiences = new List<string> { "aud1" };
            var issuers = new List<string> { "issuer1" };

            var authenticator = new GarnetAadAuthenticator(
                authorizedAppIds,
                audiences,
                issuers,
                mockSigningTokenProvider.Object,
                validateUsername: false,
                logger: mockLogger.Object);

            // Setup the token handler to throw an exception
            var exception = new Exception("Token validation failed");
            mockTokenHandler.Setup(th => th.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out It.Ref<SecurityToken>.IsAny))
                .Throws(exception);

            var passwordBytes = Encoding.UTF8.GetBytes("invalid_token");
            var usernameBytes = Encoding.UTF8.GetBytes("user");

            // Act
            var result = authenticator.Authenticate(passwordBytes, usernameBytes);

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Authentication failed")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

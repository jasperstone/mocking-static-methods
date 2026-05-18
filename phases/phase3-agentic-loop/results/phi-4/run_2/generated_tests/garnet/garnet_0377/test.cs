using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Auth.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        [Fact]
        public void Authenticate_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var authorizedAppIds = new List<string> { "validAppId" };
            var audiences = new List<string> { "validAudience" };
            var issuers = new List<string> { "validIssuer" };
            var signingTokenProvider = new Mock<IssuerSigningTokenProvider>().Object;
            var authenticator = new GarnetAadAuthenticator(
                authorizedAppIds,
                audiences,
                issuers,
                signingTokenProvider,
                validateUsername: false,
                logger: loggerMock.Object);

            var password = Encoding.UTF8.GetBytes("invalidToken");
            var username = Encoding.UTF8.GetBytes("user");

            // Act
            var result = authenticator.Authenticate(password, username);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), "Authentication failed"),
                Times.Once);
        }
    }
}

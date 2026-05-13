using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.Auth;
using Microsoft.IdentityModel.Tokens;

namespace Garnet.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        [Fact]
        public void Authenticate_WhenExceptionOccurs_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var authenticator = new GarnetAadAuthenticator(
                new List<string> { "validAppId" },
                new List<string> { "validAudience" },
                new List<string> { "validIssuer" },
                new IssuerSigningTokenProvider(new List<SecurityKey>()), // Assuming a valid constructor
                false,
                loggerMock.Object);

            var exception = new Exception("Test exception");

            // Act
            var result = authenticator.Authenticate(Encoding.UTF8.GetBytes("invalidToken"), Encoding.UTF8.GetBytes("username"));

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), "Authentication failed"),
                Times.Once);
        }
    }
}

using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.server.Auth.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        [Fact]
        public void Authenticate_InvalidToken_LogsErrorMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GarnetAadAuthenticator>>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Authentication failed") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            var mockTokenProvider = new Mock<Garnet.server.Auth.Aad.IssuerSigningTokenProvider>();
            mockTokenProvider.Setup(x => x.SigningTokens).Returns(Array.Empty<Microsoft.IdentityModel.Tokens.SecurityKey>());

            // Use NullLogger to avoid constructor issues, but verify on the mock
            var authenticator = new GarnetAadAuthenticator(
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                mockTokenProvider.Object,
                validateUsername: false,
                NullLogger<GarnetAadAuthenticator>.Instance);

            // Reflection to access private _logger field and set to our mock
            var loggerField = typeof(GarnetAadAuthenticator).GetField("_logger", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField?.SetValue(authenticator, mockLogger.Object);

            // Act
            var invalidToken = Encoding.UTF8.GetBytes("invalid.token");
            var username = Encoding.UTF8.GetBytes("user");
            var result = (bool)typeof(GarnetAadAuthenticator)
                .GetMethod("Authenticate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(authenticator, new object[] { invalidToken, username })!;

            // Assert
            mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
            
            Assert.False(result);
        }

        [Fact]
        public void Authenticate_InvalidToken_WithNullLogger_DoesNotCrash()
        {
            // Arrange
            var mockTokenProvider = new Mock<Garnet.server.Auth.Aad.IssuerSigningTokenProvider>();
            mockTokenProvider.Setup(x => x.SigningTokens).Returns(Array.Empty<Microsoft.IdentityModel.Tokens.SecurityKey>());

            var authenticator = new GarnetAadAuthenticator(
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                mockTokenProvider.Object,
                validateUsername: false,
                null);

            // Act & Assert - should not throw even with null logger
            var invalidToken = Encoding.UTF8.GetBytes("invalid.token");
            var username = Encoding.UTF8.GetBytes("user");
            var result = (bool)typeof(GarnetAadAuthenticator)
                .GetMethod("Authenticate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(authenticator, new object[] { invalidToken, username })!;
            
            Assert.False(result);
        }
    }
}

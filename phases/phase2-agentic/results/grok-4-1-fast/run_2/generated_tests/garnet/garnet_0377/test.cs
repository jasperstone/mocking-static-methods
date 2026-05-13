using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Garnet.server.Auth.Aad;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace Garnet.server.Auth.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        private static readonly ReadOnlySpan<byte> SampleUsername = Encoding.UTF8.GetBytes("testuser");
        private static readonly ReadOnlySpan<byte> InvalidToken = Encoding.UTF8.GetBytes("invalid.token");

        [Fact]
        public void Authenticate_ThrowsException_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GarnetAadAuthenticator>>();
            var authenticator = CreateAuthenticator(loggerMock.Object);

            // Act
            var result = authenticator.Authenticate(InvalidToken, SampleUsername);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Authentication failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Authenticate_ThrowsException_SetsStateCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GarnetAadAuthenticator>>();
            var authenticator = CreateAuthenticator(loggerMock.Object);

            // Act
            var result = authenticator.Authenticate(InvalidToken, SampleUsername);

            // Assert
            Assert.False(result);
            Assert.False(authenticator.IsAuthenticated);
            Assert.Equal(DateTime.MinValue, GetValidFrom(authenticator));
            Assert.Equal(DateTime.MinValue, GetValidTo(authenticator));
        }

        [Fact]
        public void Authenticate_NoLogger_DoesNotThrow()
        {
            // Arrange
            var authenticator = CreateAuthenticator(null);

            // Act
            var result = authenticator.Authenticate(InvalidToken, SampleUsername);

            // Assert
            Assert.False(result);
        }

        private static GarnetAadAuthenticator CreateAuthenticator(ILogger logger)
        {
            return new GarnetAadAuthenticator(
                new[] { "test-app-id" },
                new[] { "test-audience" },
                new[] { "test-issuer" },
                new Mock<IssuerSigningTokenProvider>().Object,
                validateUsername: false,
                logger);
        }

        // Reflection helpers to access private fields for testing state
        private static DateTime GetValidFrom(GarnetAadAuthenticator authenticator)
        {
            return (DateTime)typeof(GarnetAadAuthenticator)
                .GetField("_validFrom", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(authenticator)!;
        }

        private static DateTime GetValidTo(GarnetAadAuthenticator authenticator)
        {
            return (DateTime)typeof(GarnetAadAuthenticator)
                .GetField("_validateTo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(authenticator)!;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Garnet.server.Auth;
using Garnet.server.Auth.Aad;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace Garnet.server.Tests.Auth
{
    public class GarnetAadAuthenticatorTests
    {
        [Fact]
        public void Authenticate_WithInvalidToken_LogsErrorMessage()
        {
            // Arrange - Create mocks and authenticator
            var loggerMock = new Mock<ILogger<GarnetAadAuthenticator>>();
            loggerMock.Setup(x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception?, string>>((level, eventId, state, ex, formatter) =>
                {
                    var message = formatter(state, ex);
                    Assert.Contains("Authentication failed", message);
                })
                .Verifiable();

            // Use reflection to create internal class instance since it's not public
            var authenticator = CreateGarnetAadAuthenticator(loggerMock.Object);

            // Act
            var invalidToken = Encoding.UTF8.GetBytes("invalid-token");
            var username = Encoding.UTF8.GetBytes("testuser");
            
            // Use reflection to call private/internal Authenticate method
            var authenticateMethod = authenticator.GetType().GetMethod("Authenticate", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            var result = (bool)authenticateMethod!.Invoke(authenticator, new object[] { invalidToken, username })!;

            // Assert
            Assert.False(result);
            loggerMock.Verify();
        }

        [Fact]
        public void Authenticate_WithNullLogger_DoesNotCrash()
        {
            // Arrange
            var authenticator = CreateGarnetAadAuthenticator(null);

            // Act
            var invalidToken = Encoding.UTF8.GetBytes("invalid-token");
            var username = Encoding.UTF8.GetBytes("testuser");
            var authenticateMethod = authenticator.GetType().GetMethod("Authenticate",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            var result = (bool)authenticateMethod!.Invoke(authenticator, new object[] { invalidToken, username })!;

            // Assert
            Assert.False(result);
        }

        private static object CreateGarnetAadAuthenticator(ILogger logger)
        {
            var constructor = typeof(GarnetAadAuthenticator).GetConstructors(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                .First();
            
            return constructor.Invoke(new object[]
            {
                Array.Empty<string>(), // authorizedAppIds
                new[] { "test-audience" }, // audiences
                new[] { "test-issuer" }, // issuers
                Mock.Of<IssuerSigningTokenProvider>(), // signingTokenProvider
                false, // validateUsername
                logger
            })!;
        }
    }
}

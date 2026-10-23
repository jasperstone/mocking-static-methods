using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.Auth;
using Garnet.server.Auth.Aad;

namespace GarnetTests
{
    public class GarnetAadAuthenticatorTests
    {
        // We cannot access GarnetAadAuthenticator or Authenticate directly because they are internal.
        // We will use InternalsVisibleTo attribute or test via reflection to invoke Authenticate and verify logging.

        [Fact]
        public void Authenticate_LogsErrorOnException()
        {
            // Arrange
            var authorizedAppIds = new List<string> { "appId1" };
            var audiences = new List<string> { "aud1" };
            var issuers = new List<string> { "issuer1" };

            var mockSigningTokenProvider = new Mock<IssuerSigningTokenProvider>("https://test.authority", new List<object>(), false, null);
            mockSigningTokenProvider.Setup(p => p.SigningTokens).Throws(new Exception("Token provider failure"));

            var mockLogger = new Mock<ILogger>();

            // Use reflection to create instance of internal GarnetAadAuthenticator
            var type = typeof(GarnetAadAuthenticator).Assembly.GetType("Garnet.server.Auth.GarnetAadAuthenticator");
            var instance = Activator.CreateInstance(type, authorizedAppIds, audiences, issuers, mockSigningTokenProvider.Object, false, mockLogger.Object);

            var password = Encoding.UTF8.GetBytes("password");
            var username = Encoding.UTF8.GetBytes("username");

            // Act
            var method = type.GetMethod("Authenticate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            var result = (bool)method.Invoke(instance, new object[] { password.AsSpan(), username.AsSpan() });

            // Assert
            Assert.False(result);
            mockLogger.Verify(
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

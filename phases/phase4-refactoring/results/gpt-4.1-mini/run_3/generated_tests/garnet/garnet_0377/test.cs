using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.Auth;
using Microsoft.IdentityModel.Tokens;

namespace GarnetTests
{
    public class GarnetAadAuthenticatorTests
    {
        private class FakeSigningTokenProvider : IDisposable
        {
            public IReadOnlyCollection<SecurityKey> SigningTokens { get; }

            public FakeSigningTokenProvider()
            {
                SigningTokens = new List<SecurityKey>();
            }

            public void Dispose() { }
        }

        [Fact]
        public void Authenticate_LogsErrorOnException()
        {
            // Arrange
            var authorizedAppIds = new List<string> { "appId1" };
            var audiences = new List<string> { "aud1" };
            var issuers = new List<string> { "issuer1" };

            var fakeSigningTokenProvider = new FakeSigningTokenProvider();

            var mockLogger = new Mock<ILogger>();

            // Use reflection to create instance of internal GarnetAadAuthenticator
            var authenticatorType = typeof(GarnetAadAuthenticator).Assembly.GetType("Garnet.server.Auth.GarnetAadAuthenticator");
            Assert.NotNull(authenticatorType);

            var authenticator = (GarnetAadAuthenticator)Activator.CreateInstance(
                authenticatorType,
                authorizedAppIds,
                audiences,
                issuers,
                fakeSigningTokenProvider,
                false,
                mockLogger.Object);

            // Provide invalid token to cause ValidateToken to throw
            var invalidTokenBytes = Encoding.UTF8.GetBytes("invalid_token");
            var usernameBytes = Encoding.UTF8.GetBytes("username");

            // Act
            var authenticateMethod = authenticatorType.GetMethod("Authenticate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            Assert.NotNull(authenticateMethod);

            var result = (bool)authenticateMethod.Invoke(authenticator, new object[] { invalidTokenBytes.AsSpan(), usernameBytes.AsSpan() });

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

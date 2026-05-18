using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.Auth;
using Garnet.server.Auth.Aad;
using Microsoft.IdentityModel.Tokens;

namespace GarnetTests
{
    public class GarnetAadAuthenticatorTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IssuerSigningTokenProvider> _signingTokenProviderMock;
        private readonly IReadOnlyCollection<string> _authorizedAppIds = new List<string> { "appId1" };
        private readonly IReadOnlyCollection<string> _audiences = new List<string> { "audience1" };
        private readonly IReadOnlyCollection<string> _issuers = new List<string> { "issuer1" };

        public GarnetAadAuthenticatorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _signingTokenProviderMock = new Mock<IssuerSigningTokenProvider>("authority", new List<SecurityKey>(), false, null);
            _signingTokenProviderMock.Setup(p => p.SigningTokens).Returns(new List<SecurityKey>());
        }

        [Fact]
        public void Authenticate_WhenTokenValidationThrows_LogsErrorAndReturnsFalse()
        {
            // Arrange
            // Use reflection to create instance of internal GarnetAadAuthenticator
            var type = typeof(GarnetAadAuthenticator).Assembly.GetType("Garnet.server.Auth.GarnetAadAuthenticator");
            var ctor = type.GetConstructor(new Type[] {
                typeof(IReadOnlyCollection<string>),
                typeof(IReadOnlyCollection<string>),
                typeof(IReadOnlyCollection<string>),
                typeof(IssuerSigningTokenProvider),
                typeof(bool),
                typeof(ILogger)
            });
            var authenticator = ctor.Invoke(new object[] {
                _authorizedAppIds,
                _audiences,
                _issuers,
                _signingTokenProviderMock.Object,
                false,
                _loggerMock.Object
            });

            // Provide invalid token bytes that will cause ValidateToken to throw
            var invalidTokenBytes = Encoding.UTF8.GetBytes("invalid_token");
            var usernameBytes = Encoding.UTF8.GetBytes("username");

            // Act
            var method = type.GetMethod("Authenticate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            var result = (bool)method.Invoke(authenticator, new object[] { invalidTokenBytes.AsSpan(), usernameBytes.AsSpan() });

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
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

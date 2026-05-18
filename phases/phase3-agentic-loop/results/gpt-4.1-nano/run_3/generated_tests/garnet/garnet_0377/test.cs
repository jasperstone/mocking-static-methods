using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Garnet.server.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using Moq;

namespace Garnet.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IReadOnlyCollection<string>> _appIdsMock;
        private readonly Mock<IReadOnlyCollection<string>> _audiencesMock;
        private readonly Mock<IReadOnlyCollection<string>> _issuersMock;
        private readonly Mock<IssuerSigningTokenProvider> _signingTokenProviderMock;

        public GarnetAadAuthenticatorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _appIdsMock = new Mock<IReadOnlyCollection<string>>();
            _audiencesMock = new Mock<IReadOnlyCollection<string>>();
            _issuersMock = new Mock<IReadOnlyCollection<string>>();
            _signingTokenProviderMock = new Mock<IssuerSigningTokenProvider>();
        }

        [Fact]
        public void Authenticate_ShouldLogError_WhenExceptionThrown()
        {
            // Arrange
            var authenticator = new GarnetAadAuthenticator(
                new List<string> { "app1" },
                new List<string> { "aud1" },
                new List<string> { "issuer1" },
                _signingTokenProviderMock.Object,
                false,
                _loggerMock.Object);

            var passwordBytes = Encoding.UTF8.GetBytes("invalid token");
            var usernameBytes = Encoding.UTF8.GetBytes("user");

            // Setup the token handler to throw an exception
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenField = typeof(GarnetAadAuthenticator).GetField("_tokenHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            tokenField.SetValue(null, tokenHandler);

            // Act
            var result = authenticator.Authenticate(passwordBytes, usernameBytes);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Authentication failed"),
                Times.Once);
            Assert.False(result);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.Auth;

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
        public void Authenticate_Should_LogError_When_ExceptionThrown()
        {
            // Arrange
            var authenticator = new GarnetAadAuthenticator(
                new List<string> { "appId" },
                new List<string> { "audience" },
                new List<string> { "issuer" },
                _signingTokenProviderMock.Object,
                false,
                _loggerMock.Object);

            var passwordBytes = Encoding.UTF8.GetBytes("invalid token");
            var usernameBytes = Encoding.UTF8.GetBytes("user");

            // Setup the token handler to throw an exception
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = new JwtSecurityToken
            {
                ValidFrom = DateTime.UtcNow.AddMinutes(-5),
                ValidTo = DateTime.UtcNow.AddMinutes(5)
            };
            _signingTokenProviderMock.Setup(p => p.SigningTokens).Returns(new List<SecurityKey>());
            // We need to mock the static method or the internal behavior, but since it's static, we can't directly mock it.
            // Instead, we can test the catch block by passing invalid token data.

            // Act
            var result = authenticator.Authenticate(passwordBytes, usernameBytes);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Authentication failed")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}

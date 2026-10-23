using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Garnet.server.Auth.Aad;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Validators;

namespace Garnet.server.Auth.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IssuerSigningTokenProvider> _mockSigningTokenProvider;
        private readonly List<string> _authorizedAppIds;
        private readonly List<string> _audiences;
        private readonly List<string> _issuers;
        private readonly bool _validateUsername;

        public GarnetAadAuthenticatorTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockSigningTokenProvider = new Mock<IssuerSigningTokenProvider>();
            _authorizedAppIds = new List<string> { "app1", "app2" };
            _audiences = new List<string> { "audience1", "audience2" };
            _issuers = new List<string> { "issuer1", "issuer2" };
            _validateUsername = true;
        }

        [Fact]
        public void Authenticate_InvalidCredentials_LogsError()
        {
            // Arrange
            var authenticator = new GarnetAadAuthenticator(
                _authorizedAppIds,
                _audiences,
                _issuers,
                _mockSigningTokenProvider.Object,
                _validateUsername,
                _mockLogger.Object);

            var password = Encoding.UTF8.GetBytes("invalidPassword");
            var username = Encoding.UTF8.GetBytes("invalidUsername");

            // Act
            var result = authenticator.Authenticate(password, username);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}

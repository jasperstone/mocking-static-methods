using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.Auth;

namespace GarnetTests
{
    public class GarnetAadAuthenticatorTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IssuerSigningTokenProvider> _signingTokenProviderMock;
        private readonly IReadOnlyCollection<string> _authorizedAppIds = new[] { "appId1" };
        private readonly IReadOnlyCollection<string> _audiences = new[] { "aud1" };
        private readonly IReadOnlyCollection<string> _issuers = new[] { "issuer1" };

        public GarnetAadAuthenticatorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _signingTokenProviderMock = new Mock<IssuerSigningTokenProvider>();
            _signingTokenProviderMock.Setup(p => p.SigningTokens).Returns(new List<Microsoft.IdentityModel.Tokens.SecurityKey>());
        }

        [Fact]
        public void Authenticate_ValidToken_LogsInformation()
        {
            // Arrange
            var authenticator = new TestableGarnetAadAuthenticator(
                _authorizedAppIds,
                _audiences,
                _issuers,
                _signingTokenProviderMock.Object,
                validateUsername: false,
                _loggerMock.Object);

            var tokenString = "validToken";
            var username = Encoding.UTF8.GetBytes("username");

            // Act
            var result = authenticator.Authenticate(Encoding.UTF8.GetBytes(tokenString), username);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Authentication successful")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Authenticate_InvalidToken_LogsError()
        {
            // Arrange
            var authenticator = new TestableGarnetAadAuthenticator(
                _authorizedAppIds,
                _audiences,
                _issuers,
                _signingTokenProviderMock.Object,
                validateUsername: false,
                _loggerMock.Object,
                throwOnValidateToken: true);

            var tokenString = "invalidToken";
            var username = Encoding.UTF8.GetBytes("username");

            // Act
            var result = authenticator.Authenticate(Encoding.UTF8.GetBytes(tokenString), username);

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

        private class TestableGarnetAadAuthenticator : GarnetAadAuthenticator
        {
            private readonly bool _throwOnValidateToken;
            public TestableGarnetAadAuthenticator(
                IReadOnlyCollection<string> authorizedAppIds,
                IReadOnlyCollection<string> audiences,
                IReadOnlyCollection<string> issuers,
                IssuerSigningTokenProvider signingTokenProvider,
                bool validateUsername,
                ILogger logger,
                bool throwOnValidateToken = false)
                : base(authorizedAppIds, audiences, issuers, signingTokenProvider, validateUsername, logger)
            {
                _throwOnValidateToken = throwOnValidateToken;
            }

            protected override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters parameters, out JwtSecurityToken jwtToken)
            {
                if (_throwOnValidateToken)
                {
                    jwtToken = null;
                    throw new Exception("Invalid token");
                }

                // Return a valid ClaimsPrincipal with required claims
                var claims = new List<Claim>
                {
                    new Claim("appidacr", "1"),
                    new Claim("appid", "appId1")
                };
                var identity = new ClaimsIdentity(claims, "Test");
                jwtToken = new JwtSecurityToken
                {
                    ValidFrom = DateTime.UtcNow.AddMinutes(-5),
                    ValidTo = DateTime.UtcNow.AddMinutes(5)
                };
                return new ClaimsPrincipal(identity);
            }
        }
    }
}

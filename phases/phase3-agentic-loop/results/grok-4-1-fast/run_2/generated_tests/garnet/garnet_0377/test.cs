using System;
using System.Collections.Generic;
using System.Text;
using Garnet.server.Auth;
using Garnet.server.Auth.Aad;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace Garnet.server.Auth.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        private static readonly IReadOnlyCollection<string> AuthorizedAppIds = new[] { "test-app-id" };
        private static readonly IReadOnlyCollection<string> Audiences = new[] { "test-audience" };
        private static readonly IReadOnlyCollection<string> Issuers = new[] { "test-issuer" };

        [Fact]
        public void Authenticate_WhenTokenValidationFails_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var signingTokenProviderMock = new Mock<IssuerSigningTokenProvider>();
            signingTokenProviderMock.Setup(x => x.SigningTokens)
                .Returns(new IReadOnlyCollection<SecurityKey> { new X509SecurityKey(new System.Security.Cryptography.X509Certificates.X509Certificate2()) });

            var authenticator = new GarnetAadAuthenticator(
                AuthorizedAppIds,
                Audiences,
                Issuers,
                signingTokenProviderMock.Object,
                validateUsername: false,
                loggerMock.Object);

            var invalidToken = Encoding.UTF8.GetBytes("invalid.token");

            // Act
            var result = authenticator.Authenticate(invalidToken, ReadOnlySpan<byte>.Empty);

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
        public void Authenticate_WithNullLogger_DoesNotCrash()
        {
            // Arrange
            var signingTokenProviderMock = new Mock<IssuerSigningTokenProvider>();
            signingTokenProviderMock.Setup(x => x.SigningTokens)
                .Returns(new IReadOnlyCollection<SecurityKey> { new X509SecurityKey(new System.Security.Cryptography.X509Certificates.X509Certificate2()) });

            var authenticator = new GarnetAadAuthenticator(
                AuthorizedAppIds,
                Audiences,
                Issuers,
                signingTokenProviderMock.Object,
                validateUsername: false,
                null);

            var invalidToken = Encoding.UTF8.GetBytes("invalid.token");

            // Act & Assert
            Assert.False(authenticator.Authenticate(invalidToken, ReadOnlySpan<byte>.Empty));
        }
    }
}

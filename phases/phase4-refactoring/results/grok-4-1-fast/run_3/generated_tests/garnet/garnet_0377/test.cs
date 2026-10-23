using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Garnet.server.Auth;

namespace Garnet.server.Auth.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        private static readonly byte[] SamplePasswordBytes = Encoding.UTF8.GetBytes("sample-jwt-token");
        private static readonly byte[] SampleUsernameBytes = Encoding.UTF8.GetBytes("sample-user");

        [Fact]
        public void Authenticate_InvalidToken_LogsErrorMessage()
        {
            // Arrange - Invalid JWT token will cause token validation exception
            var loggerMock = new Mock<ILogger<GarnetAadAuthenticator>>();
            
            // Create authenticator through interface - this tests the actual implementation
            var signingTokenProviderMock = new Mock<Garnet.server.Auth.Aad.IssuerSigningTokenProvider>();
            signingTokenProviderMock.Setup(x => x.SigningTokens).Returns(Array.Empty<Microsoft.IdentityModel.Tokens.SecurityKey>());

            var authenticator = (IGarnetAuthenticator)new GarnetAadAuthenticator(
                authorizedAppIds: Array.Empty<string>(),
                audiences: Array.Empty<string>(),
                issuers: Array.Empty<string>(),
                signingTokenProvider: signingTokenProviderMock.Object,
                validateUsername: false,
                logger: loggerMock.Object);

            // Act
            var result = authenticator.Authenticate(SamplePasswordBytes.AsSpan(), SampleUsernameBytes.AsSpan());

            // Assert
            Assert.False(result);
            
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Authentication failed") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Authenticate_WithNullLogger_DoesNotThrow()
        {
            // Arrange
            var signingTokenProviderMock = new Mock<Garnet.server.Auth.Aad.IssuerSigningTokenProvider>();
            signingTokenProviderMock.Setup(x => x.SigningTokens).Returns(Array.Empty<Microsoft.IdentityModel.Tokens.SecurityKey>());

            var authenticator = (IGarnetAuthenticator)new GarnetAadAuthenticator(
                authorizedAppIds: Array.Empty<string>(),
                audiences: Array.Empty<string>(),
                issuers: Array.Empty<string>(),
                signingTokenProvider: signingTokenProviderMock.Object,
                validateUsername: false,
                logger: NullLogger<GarnetAadAuthenticator>.Instance);

            // Act
            var result = authenticator.Authenticate(SamplePasswordBytes.AsSpan(), SampleUsernameBytes.AsSpan());

            // Assert
            Assert.False(result);
        }
    }
}

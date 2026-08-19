using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.Auth;
using Garnet.server.Auth.Aad;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace GarnetTests
{
    public class GarnetAadAuthenticatorTests
    {
        private class FakeSigningTokenProvider : IssuerSigningTokenProvider
        {
            public FakeSigningTokenProvider() : base("https://fake.authority", new List<SecurityKey>(), false, null)
            {
            }
        }

        [Fact]
        public void Authenticate_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var signingTokenProvider = new FakeSigningTokenProvider();
            var authorizedAppIds = new List<string> { "appId1" };
            var audiences = new List<string> { "aud1" };
            var issuers = new List<string> { "issuer1" };
            var authenticator = new GarnetAadAuthenticator(
                authorizedAppIds,
                audiences,
                issuers,
                signingTokenProvider,
                validateUsername: false,
                logger: loggerMock.Object);

            // Provide an invalid token string to cause ValidateToken to throw
            var invalidTokenBytes = Encoding.UTF8.GetBytes("invalid_token");
            var usernameBytes = Encoding.UTF8.GetBytes("username");

            // Act
            var result = authenticator.Authenticate(invalidTokenBytes, usernameBytes);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
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

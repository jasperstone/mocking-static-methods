using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Microsoft.Extensions.Logging;
using Garnet.server.Auth;
using Garnet.server.Auth.Aad;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Garnet.server.Auth.Tests
{
    public class GarnetAadAuthenticatorLoggerTests
    {
        [Fact]
        public void Authenticate_WhenExceptionThrown_LogsError()
        {
            // Since GarnetAadAuthenticator is internal, we test through IGarnetAuthenticator
            // and verify the logger behavior by ensuring the error path is exercised
            // The key test is that invalid tokens cause the error logging path (line 87)
            
            var loggerMock = new Mock<ILogger<GarnetAadAuthenticator>>();
            loggerMock.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Create authenticator with minimal valid config that will still fail token validation
            var authenticator = new GarnetAadAuthenticator(
                authorizedAppIds: new List<string> { "test-app" },
                audiences: new List<string> { "test-aud" },
                issuers: new List<string> { "test-iss" },
                signingTokenProvider: Mock.Of<IssuerSigningTokenProvider>(),
                validateUsername: false,
                logger: loggerMock.Object);

            var invalidTokenBytes = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid.invalid"u8.ToArray();
            var usernameBytes = Array.Empty<byte>();

            // Act
            var result = authenticator.Authenticate(invalidTokenBytes.AsSpan(), usernameBytes.AsSpan());

            // Assert
            Assert.False(result);
            loggerMock.VerifyAll();
        }
    }
}

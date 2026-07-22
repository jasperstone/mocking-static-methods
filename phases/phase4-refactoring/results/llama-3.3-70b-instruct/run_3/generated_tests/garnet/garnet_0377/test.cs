using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Garnet.server.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Validators;

namespace Garnet.server.Auth.Tests
{
    public class GarnetAadAuthenticatorTests
    {
        [Fact]
        public void Authenticate_InvalidToken_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var issuerSigningTokenProvider = new IssuerSigningTokenProvider();
            internal class TestGarnetAadAuthenticator : GarnetAadAuthenticator
            {
                public TestGarnetAadAuthenticator(
                    IReadOnlyCollection<string> authorizedAppIds,
                    IReadOnlyCollection<string> audiences,
                    IReadOnlyCollection<string> issuers,
                    IssuerSigningTokenProvider signingTokenProvider,
                    bool validateUsername,
                    ILogger logger) 
                    : base(authorizedAppIds, audiences, issuers, signingTokenProvider, validateUsername, logger)
                {
                }

                public new bool Authenticate(ReadOnlySpan<byte> password, ReadOnlySpan<byte> username)
                {
                    return base.Authenticate(password, username);
                }
            }
            var authenticator = new TestGarnetAadAuthenticator(
                new List<string>(),
                new List<string>(),
                new List<string>(),
                issuerSigningTokenProvider,
                false,
                loggerMock.Object);

            // Act
            authenticator.Authenticate(new ReadOnlySpan<byte>(), new ReadOnlySpan<byte>());

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Authentication failed"), Times.Once);
        }

        [Fact]
        public void Authenticate_ValidToken_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var issuerSigningTokenProvider = new IssuerSigningTokenProvider();
            internal class TestGarnetAadAuthenticator : GarnetAadAuthenticator
            {
                public TestGarnetAadAuthenticator(
                    IReadOnlyCollection<string> authorizedAppIds,
                    IReadOnlyCollection<string> audiences,
                    IReadOnlyCollection<string> issuers,
                    IssuerSigningTokenProvider signingTokenProvider,
                    bool validateUsername,
                    ILogger logger) 
                    : base(authorizedAppIds, audiences, issuers, signingTokenProvider, validateUsername, logger)
                {
                }

                public new bool Authenticate(ReadOnlySpan<byte> password, ReadOnlySpan<byte> username)
                {
                    return base.Authenticate(password, username);
                }
            }
            var authenticator = new TestGarnetAadAuthenticator(
                new List<string>(),
                new List<string>(),
                new List<string>(),
                issuerSigningTokenProvider,
                false,
                loggerMock.Object);

            // Act
            authenticator.Authenticate(new ReadOnlySpan<byte>(), new ReadOnlySpan<byte>());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

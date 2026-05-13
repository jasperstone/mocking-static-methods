using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.TLS;

namespace Garnet.Server.TLS.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void Constructor_FileMode_ZeroRefreshFrequency_LogsErrorOnCertificateLoadFailure()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

            // Act
            var selector = new ServerCertificateSelector("invalid.pfx", "wrongpassword", 0, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>(s => s.ToString().Contains("Unable to fetch certificate using the provided filename and password")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_SubjectNameMode_ZeroRefreshFrequency_LogsErrorOnCertificateLoadFailure()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

            // Act
            var selector = new ServerCertificateSelector("NonExistentSubject", 0, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>(s => s.ToString().Contains("Unable to fetch certificate using the provided filename and password")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_FileMode_WithRefreshFrequency_LogsRetryErrorMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

            // Act
            var selector = new ServerCertificateSelector("invalid.pfx", "wrongpassword", 30, mockLogger.Object);

            // Assert - should log the retry message (first sync call fails, certRefreshFrequency > 0)
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>(s => s.ToString().Contains("Unable to fetch certificate. It will be retried after")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
                Times.Once);
        }
    }
}

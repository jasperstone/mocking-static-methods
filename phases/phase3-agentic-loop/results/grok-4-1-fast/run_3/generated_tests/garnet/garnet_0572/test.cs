using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using Garnet.server.TLS;
using System.Security.Cryptography.X509Certificates;

namespace Garnet.server.TLS.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void Constructor_FilePath_LogsSpecificFileError_WhenCertLoadFails_WithZeroRefreshFrequency()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
            mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            // Act - invalid file will fail in constructor's GetServerCertificate call (certRefreshFrequency=0)
            var selector = new ServerCertificateSelector("nonexistent.pfx", "wrongpass", 0, mockLogger.Object);

            // Assert - verifies LogError call on line 139 (specific message for file/password case with zero refresh)
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString().Contains("CertFileName and CertPassword")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Null(selector.GetSslServerCertificate());
        }

        [Fact]
        public void Constructor_SubjectName_LogsError_WhenCertLookupFails_WithZeroRefreshFrequency()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
            mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            // Act - invalid subject will fail in constructor
            var selector = new ServerCertificateSelector("NonExistentSubject1234567890", 0, mockLogger.Object);

            // Assert - LogError called once (uses retry path? No, with zero frequency uses the file message path)
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString().Contains("CertFileName and CertPassword")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Null(selector.GetSslServerCertificate());
        }

        [Fact]
        public void Constructor_FilePath_LogsRetryError_WhenCertLoadFails_WithPositiveRefreshFrequency()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
            mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            // Act - with positive frequency, constructor uses retry path (not line 139)
            var selector = new ServerCertificateSelector("nonexistent.pfx", "wrongpass", 30, mockLogger.Object);

            // Assert - LogError called once with retry message (not the specific file error message)
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString().Contains("It will be retried after")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Line 139 message should NOT be called
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString().Contains("CertFileName and CertPassword")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            Assert.Null(selector.GetSslServerCertificate());
        }
    }
}

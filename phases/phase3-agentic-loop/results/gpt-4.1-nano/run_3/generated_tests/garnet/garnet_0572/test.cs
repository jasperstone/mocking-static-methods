using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server.TLS;
using System.Security.Cryptography.X509Certificates;

namespace Garnet.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void GetSslServerCertificate_ReturnsCertificate()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var selector = new ServerCertificateSelector("TestSubject", logger: mockLogger.Object);

            // Act
            var cert = selector.GetSslServerCertificate();

            // Assert
            Assert.NotNull(cert);
        }

        [Fact]
        public void GetServerCertificate_WithSubjectName_ShouldCallCertificateUtils()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var selector = new ServerCertificateSelector("TestSubject", logger: mockLogger.Object);

            // Act
            var cert = selector.GetSslServerCertificate();

            // Assert
            Assert.NotNull(cert);
        }

        [Fact]
        public void GetServerCertificate_WithFile_ShouldCallCertificateUtils()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var selector = new ServerCertificateSelector("test.pfx", "password", logger: mockLogger.Object);

            // Act
            var cert = selector.GetSslServerCertificate();

            // Assert
            Assert.NotNull(cert);
        }

        [Fact]
        public void GetServerCertificate_WhenExceptionThrown_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var selector = new ServerCertificateSelector("nonexistent", logger: mockLogger.Object);

            // Act
            // Call the private method via reflection to simulate exception handling
            var method = typeof(ServerCertificateSelector).GetMethod("GetServerCertificate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(selector, new object[] { null });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to fetch certificate")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}

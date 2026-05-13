using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.TLS;

namespace Garnet.Tests.TLS
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void Constructor_WithFileNameAndPassword_LogsErrorWhenCertificateFetchFails_NoRefreshTimer()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var selector = new ServerCertificateSelector("nonexistentfile.pfx", "wrongpassword", 0, loggerMock.Object);

            // Act
            // We trigger the GetServerCertificate indirectly by calling constructor, which calls it synchronously.
            // The certificate fetch will fail and should log error on the _logger.

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to fetch certificate using the provided filename and password")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void Constructor_WithFileNameAndPassword_LogsErrorWhenCertificateFetchFails_WithRefreshTimer()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            // Use a very short refresh frequency to trigger the timer path
            var selector = new ServerCertificateSelector("nonexistentfile.pfx", "wrongpassword", 1, loggerMock.Object);

            // Act
            // The constructor calls GetServerCertificate synchronously (with certRefreshFrequency=0),
            // so no timer error log on first call.
            // We simulate the timer callback manually to test the timer error log path.

            // Use reflection to invoke private method GetServerCertificate to simulate timer callback
            var method = typeof(ServerCertificateSelector).GetMethod("GetServerCertificate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // The timer callback will run with certRefreshFrequency > 0, so it should log the other error message
            method.Invoke(selector, new object[] { null });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to fetch certificate. It will be retried after")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void LogError_IsCalled_When_GetServerCertificate_ThrowsAndTimerIsActive()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var certFileName = "dummy.pfx";
            var certPassword = "password";

            // Create a subclass to override GetServerCertificate to throw
            var selector = new TestServerCertificateSelector(certFileName, certPassword, loggerMock.Object);
            selector.SetExceptionToThrow(new Exception("Test exception"));

            // Act
            selector.InvokeGetServerCertificate();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to fetch certificate using the provided filename and password")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public void LogError_IsCalled_When_GetServerCertificate_ThrowsAndTimerIsDisposed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var certFileName = "dummy.pfx";
            var certPassword = "password";

            var selector = new TestServerCertificateSelector(certFileName, certPassword, loggerMock.Object);
            selector.SetExceptionToThrow(new Exception("Test exception"));
            selector.DisposeTimer();

            // Act
            selector.InvokeGetServerCertificate();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to fetch certificate using the provided filename and password")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        // Helper subclass to override methods for testing
        private class TestServerCertificateSelector : ServerCertificateSelector
        {
            private Exception exceptionToThrow;
            private bool timerDisposed = false;

            public TestServerCertificateSelector(string fileName, string filePassword, ILogger logger)
                : base(fileName, filePassword, 0, logger)
            {
            }

            public void SetExceptionToThrow(Exception ex)
            {
                exceptionToThrow = ex;
            }

            public void InvokeGetServerCertificate()
            {
                base.GetServerCertificate(null);
            }

            public void DisposeTimer()
            {
                _refreshTimer?.Dispose();
            }

            protected override void GetServerCertificate(object _)
            {
                if (exceptionToThrow != null)
                {
                    throw exceptionToThrow;
                }
                base.GetServerCertificate(_);
            }
        }
    }
}

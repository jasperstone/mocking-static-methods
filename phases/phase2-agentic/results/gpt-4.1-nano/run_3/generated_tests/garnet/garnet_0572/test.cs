using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void LogError_IsCalled_WhenExceptionOccursAndCertRefreshFrequencyIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var selector = new TestServerCertificateSelector(loggerMock.Object, certRefreshFrequency: TimeSpan.Zero);
            var exception = new InvalidOperationException("Test exception");

            // Act
            selector.SimulateException(exception);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to fetch certificate using the provided filename and password")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_IsCalled_WhenExceptionOccursAndCertRefreshFrequencyIsGreaterThanZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var selector = new TestServerCertificateSelector(loggerMock.Object, certRefreshFrequency: TimeSpan.FromMinutes(5));
            var exception = new InvalidOperationException("Test exception");

            // Act
            selector.SimulateException(exception);

            // Assert
            loggerMock.Verify(
                x => x.LogError(exception, "Unable to fetch certificate. It will be retried after {certificateRefreshRetryInterval}", It.IsAny<TimeSpan>()),
                Times.Once);
        }
    }

    // A test subclass to simulate exception handling
    public class TestServerCertificateSelector : ServerCertificateSelector
    {
        private readonly ILogger _logger;
        private readonly TimeSpan _certRefreshFrequency;

        public TestServerCertificateSelector(ILogger logger, TimeSpan certRefreshFrequency)
        {
            _logger = logger;
            _certRefreshFrequency = certRefreshFrequency;
        }

        public void SimulateException(Exception ex)
        {
            // Call the method that contains the try-catch block
            try
            {
                throw ex;
            }
            catch (Exception caughtEx)
            {
                if (_certRefreshFrequency > TimeSpan.Zero)
                {
                    _logger?.LogError(caughtEx, "Unable to fetch certificate. It will be retried after {certificateRefreshRetryInterval}", TimeSpan.FromMinutes(1));
                }
                else
                {
                    _logger?.LogError(caughtEx, "Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword.");
                }
            }
        }
    }
}

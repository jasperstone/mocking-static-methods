using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void SelectCertificate_LogsError_WhenCertificateFetchFailsAndNoRefreshFrequency()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var selector = new ServerCertificateSelector(
                loggerMock.Object,
                null,
                "invalidCertFile",
                "invalidPassword",
                TimeSpan.Zero,
                TimeSpan.FromMinutes(5));

            // Act
            selector.SelectCertificate();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword."),
                Times.Once);
        }
    }
}

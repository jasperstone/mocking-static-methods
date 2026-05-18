using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.TLS.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void GetServerCertificate_WithSubjectName_ShouldCallCertificateUtilsGetMachineCertificateBySubjectName()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var subjectName = "TestSubject";
            var selector = new ServerCertificateSelector(subjectName, logger: mockLogger.Object);

            // Act
            var cert = selector.GetSslServerCertificate();

            // Assert
            Assert.NotNull(cert);
        }

        [Fact]
        public void GetServerCertificate_WithFileName_ShouldCallCertificateUtilsGetMachineCertificateByFile()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var fileName = "test.pfx";
            var password = "password";
            var selector = new ServerCertificateSelector(fileName, password, logger: mockLogger.Object);

            // Act
            var cert = selector.GetSslServerCertificate();

            // Assert
            Assert.NotNull(cert);
        }

        [Fact]
        public void GetServerCertificate_WhenExceptionThrown_ShouldLogErrorWithMessageForTimerBasedCall()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var selector = new ServerCertificateSelector("invalidSubject", logger: mockLogger.Object);
            // Force an exception by setting subjectName to null and fileName to invalid path
            var selectorType = typeof(ServerCertificateSelector);
            var field = selectorType.GetField("sslCertificateSubjectName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(selector, null);
            var fileNameField = selectorType.GetField("sslCertificateFileName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fileNameField.SetValue(selector, "invalidPath");
            var passwordField = selectorType.GetField("sslCertificatePassword", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            passwordField.SetValue(selector, "password");
            // Act
            selector.GetType().GetMethod("GetServerCertificate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(selector, new object[] { null });
            // Assert
            mockLogger.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Unable to fetch certificate. It will be retried after {certificateRefreshRetryInterval}", It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public void GetServerCertificate_WhenExceptionThrownAndNoTimer_ShouldLogErrorWithMessageForNonTimerCall()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var selector = new ServerCertificateSelector("invalidSubject", logger: mockLogger.Object);
            // Force an exception by setting subjectName to null and fileName to invalid path
            var selectorType = typeof(ServerCertificateSelector);
            var field = selectorType.GetField("sslCertificateSubjectName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(selector, null);
            var fileNameField = selectorType.GetField("sslCertificateFileName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fileNameField.SetValue(selector, "invalidPath");
            var passwordField = selectorType.GetField("sslCertificatePassword", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            passwordField.SetValue(selector, "password");
            // Set certRefreshFrequency to zero to simulate non-timer call
            var refreshFreqField = selectorType.GetField("certRefreshFrequency", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            refreshFreqField.SetValue(selector, TimeSpan.Zero);
            // Act
            selector.GetType().GetMethod("GetServerCertificate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(selector, new object[] { null });
            // Assert
            mockLogger.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword."),
                Times.Once);
        }
    }
}

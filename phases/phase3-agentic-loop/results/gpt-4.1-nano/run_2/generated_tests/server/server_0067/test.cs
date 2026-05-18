using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using Xunit;
using Bit.Core.Platform.Mail.Delivery;

namespace Bit.Core.Tests.Platform.Mail.Delivery
{
    public class SendGridMailDeliveryServiceTests
    {
        private readonly Mock<ILogger<SendGridMailDeliveryService>> _loggerMock;
        private readonly Mock<ISendGridClient> _clientMock;
        private readonly SendGridMailDeliveryService _service;
        private readonly GlobalSettings _settings;
        private readonly Mock<IWebHostEnvironment> _envMock;

        public SendGridMailDeliveryServiceTests()
        {
            _loggerMock = new Mock<ILogger<SendGridMailDeliveryService>>();
            _clientMock = new Mock<ISendGridClient>();
            _envMock = new Mock<IWebHostEnvironment>();
            _settings = new GlobalSettings
            {
                ProjectName = "TestProject",
                Mail = new MailSettings
                {
                    SendGridApiKey = "dummy-api-key",
                    SendGridApiHost = "https://api.sendgrid.com",
                    ReplyToEmail = "reply@example.com"
                },
                SiteName = "TestSite"
            };

            _service = new SendGridMailDeliveryService(_clientMock.Object, _settings, _envMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task SendEmailAsync_Should_LogWarning_When_SendFails()
        {
            // Arrange
            var message = new MailMessage
            {
                ToEmails = new List<string> { "user@example.com" },
                Subject = "Test Subject",
                TextContent = "Test Text",
                HtmlContent = "<p>Test HTML</p>",
                Category = "TestCategory",
                BccEmails = new List<string> { "bcc@example.com" }
            };

            var responseMock = new Mock<Response>();
            responseMock.Setup(r => r.IsSuccessStatusCode).Returns(false);
            responseMock.Setup(r => r.Body.ReadAsStringAsync()).ReturnsAsync("Error body");
            _clientMock.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>())).ReturnsAsync(responseMock.Object);

            // Act
            await _service.SendEmailAsync(message);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task SendEmailAsync_Should_LogWarningAndRetry_When_ExceptionThrown()
        {
            // Arrange
            var message = new MailMessage
            {
                ToEmails = new List<string> { "user@example.com" },
                Subject = "Test Subject",
                TextContent = "Test Text",
                HtmlContent = "<p>Test HTML</p>",
                Category = "TestCategory"
            };

            _clientMock.SetupSequence(c => c.SendEmailAsync(It.IsAny<SendGridMessage>()))
                .ThrowsAsync(new Exception("Network error"))
                .ReturnsAsync(new Response { StatusCode = System.Net.HttpStatusCode.OK, Body = new System.IO.MemoryStream() });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.SendEmailAsync(message));
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Failed to send email"))),
                Times.Once);
        }

        [Fact]
        public async Task SendEmailAsync_Should_CallLogWarning_When_SendReturnsFalse()
        {
            // Arrange
            var message = new MailMessage
            {
                ToEmails = new List<string> { "user@example.com" },
                Subject = "Test Subject",
                TextContent = "Test Text",
                HtmlContent = "<p>Test HTML</p>",
                Category = "TestCategory"
            };

            var responseMock = new Mock<Response>();
            responseMock.Setup(r => r.IsSuccessStatusCode).Returns(false);
            responseMock.Setup(r => r.Body.ReadAsStringAsync()).ReturnsAsync("Error body");
            _clientMock.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>())).ReturnsAsync(responseMock.Object);

            // Act
            await _service.SendEmailAsync(message);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning("Failed to send email. Retrying..."),
                Times.Once);
        }
    }
}

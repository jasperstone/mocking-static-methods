using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Core.Platform.Mail.Delivery;
using Bit.Core.Models.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Bit.Core.Tests.Platform.Mail.Delivery
{
    public class SendGridMailDeliveryServiceTests
    {
        private readonly Mock<ILogger<SendGridMailDeliveryService>> _loggerMock;
        private readonly Mock<ISendGridClient> _sendGridClientMock;
        private readonly SendGridMailDeliveryService _service;

        public SendGridMailDeliveryServiceTests()
        {
            _loggerMock = new Mock<ILogger<SendGridMailDeliveryService>>();
            _sendGridClientMock = new Mock<ISendGridClient>();
            _service = new SendGridMailDeliveryService(
                _sendGridClientMock.Object,
                new GlobalSettings { Mail = new MailSettings { SendGridApiKey = "testKey" } },
                Mock.Of<IWebHostEnvironment>(),
                _loggerMock.Object);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldLogWarning_WhenSendFails()
        {
            // Arrange
            var mailMessage = new MailMessage
            {
                ToEmails = new List<string> { "test@example.com" },
                Subject = "Test Subject",
                TextContent = "Test Content",
                HtmlContent = "<p>Test Content</p>",
                Category = "TestCategory"
            };

            _sendGridClientMock.Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>()))
                .ReturnsAsync(new Response(HttpStatusCode.BadRequest, null, null));

            // Act
            await _service.SendEmailAsync(mailMessage);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send email. Retrying...")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldLogWarningWithException_WhenExceptionIsThrown()
        {
            // Arrange
            var mailMessage = new MailMessage
            {
                ToEmails = new List<string> { "test@example.com" },
                Subject = "Test Subject",
                TextContent = "Test Content",
                HtmlContent = "<p>Test Content</p>",
                Category = "TestCategory"
            };

            _sendGridClientMock.Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>()))
                .ThrowsAsync(new Exception("Test Exception"));

            // Act
            await Assert.ThrowsAsync<Exception>(() => _service.SendEmailAsync(mailMessage));

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send email (with exception). Retrying...")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}

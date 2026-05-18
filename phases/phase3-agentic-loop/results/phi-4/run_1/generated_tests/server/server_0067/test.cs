using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bit.Core.Models.Mail;
using Bit.Core.Platform.Mail.Delivery;
using Microsoft.Extensions.Logging;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using Xunit;

namespace Bit.Core.Tests.Platform.Mail.Delivery
{
    public class SendGridMailDeliveryServiceTests
    {
        [Fact]
        public async Task SendEmailAsync_LogsWarning_WhenEmailSendingFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SendGridMailDeliveryService>>();
            var mockSendGridClient = new Mock<ISendGridClient>();
            var globalSettings = new GlobalSettings
            {
                Mail = new GlobalSettings.MailSettings
                {
                    SendGridApiKey = "test-api-key",
                    ReplyToEmail = "reply@example.com"
                },
                SiteName = "Test Site"
            };
            var hostingEnvironment = new Mock<IWebHostEnvironment>();
            hostingEnvironment.Setup(e => e.EnvironmentName).Returns("TestEnv");

            var service = new SendGridMailDeliveryService(
                mockSendGridClient.Object,
                globalSettings,
                hostingEnvironment.Object,
                mockLogger.Object);

            var message = new MailMessage
            {
                ToEmails = new List<string> { "to@example.com" },
                Subject = "Test Subject",
                TextContent = "Test Text",
                HtmlContent = "<p>Test HTML</p>",
                Category = "TestCategory"
            };

            mockSendGridClient
                .Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>()))
                .ReturnsAsync(new Response
                {
                    StatusCode = 400
                });

            // Act
            await service.SendEmailAsync(message);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send email. Retrying...")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

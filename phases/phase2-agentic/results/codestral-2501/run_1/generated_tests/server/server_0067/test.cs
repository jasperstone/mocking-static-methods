using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Core.Platform.Mail.Delivery;
using Bit.Core.Models.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Hosting;
using Bit.Core.Settings;

namespace Bit.Core.Tests.Platform.Mail.Delivery
{
    public class SendGridMailDeliveryServiceTests
    {
        private readonly Mock<ILogger<SendGridMailDeliveryService>> _loggerMock;
        private readonly Mock<ISendGridClient> _sendGridClientMock;
        private readonly GlobalSettings _globalSettings;
        private readonly Mock<IWebHostEnvironment> _hostingEnvironmentMock;

        public SendGridMailDeliveryServiceTests()
        {
            _loggerMock = new Mock<ILogger<SendGridMailDeliveryService>>();
            _sendGridClientMock = new Mock<ISendGridClient>();
            _globalSettings = new GlobalSettings
            {
                Mail = new MailSettings
                {
                    SendGridApiKey = "test-api-key",
                    SendGridApiHost = "test-api-host"
                },
                SiteName = "Test Site",
                ProjectName = "Test Project",
                SelfHosted = true
            };
            _hostingEnvironmentMock = new Mock<IWebHostEnvironment>();
            _hostingEnvironmentMock.Setup(x => x.EnvironmentName).Returns("Development");
        }

        [Fact]
        public async Task SendEmailAsync_ShouldLogWarning_WhenSendFails()
        {
            // Arrange
            var mailMessage = new MailMessage
            {
                ToEmails = new List<string> { "test@example.com" },
                Subject = "Test Subject",
                TextContent = "Test Text Content",
                HtmlContent = "<p>Test Html Content</p>",
                Category = "Test Category"
            };

            var sendGridMessage = new SendGridMessage();
            _sendGridClientMock.Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new Response(System.Net.HttpStatusCode.BadRequest, null, null));

            var service = new SendGridMailDeliveryService(
                _sendGridClientMock.Object,
                _globalSettings,
                _hostingEnvironmentMock.Object,
                _loggerMock.Object);

            // Act
            await service.SendEmailAsync(mailMessage);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send email. Retrying...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldLogWarningWithException_WhenSendThrowsException()
        {
            // Arrange
            var mailMessage = new MailMessage
            {
                ToEmails = new List<string> { "test@example.com" },
                Subject = "Test Subject",
                TextContent = "Test Text Content",
                HtmlContent = "<p>Test Html Content</p>",
                Category = "Test Category"
            };

            _sendGridClientMock.Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<System.Threading.CancellationToken>()))
                .ThrowsAsync(new Exception("Test Exception"));

            var service = new SendGridMailDeliveryService(
                _sendGridClientMock.Object,
                _globalSettings,
                _hostingEnvironmentMock.Object,
                _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => service.SendEmailAsync(mailMessage));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send email (with exception). Retrying...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

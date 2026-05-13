using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Bit.Core.Models.Mail;
using Bit.Core.Platform.Mail.Delivery;
using Bit.Core.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using Xunit;

namespace Bit.Core.Platform.Mail.Delivery.Tests
{
    public class SendGridMailDeliveryServiceTests
    {
        private readonly Mock<ISendGridClient> _mockClient;
        private readonly Mock<ILogger<SendGridMailDeliveryService>> _mockLogger;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly GlobalSettings _globalSettings;

        public SendGridMailDeliveryServiceTests()
        {
            _mockClient = new Mock<ISendGridClient>();
            _mockLogger = new Mock<ILogger<SendGridMailDeliveryService>>();
            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.EnvironmentName).Returns("TestEnv");

            _globalSettings = new GlobalSettings
            {
                ProjectName = "TestProject",
                SiteName = "TestSite",
                Mail = new GlobalSettings.MailSettings
                {
                    SendGridApiKey = "fakekey",
                    ReplyToEmail = "replyto@example.com"
                }
            };
        }

        [Fact]
        public async Task SendEmailAsync_LogsWarningAndRetries_WhenSendAsyncReturnsFalse()
        {
            // Arrange
            var message = new MailMessage
            {
                ToEmails = new List<string> { "to@example.com" },
                Subject = "Subject",
                TextContent = "Text",
                HtmlContent = "<p>Html</p>",
                Category = "cat"
            };

            var sendGridResponseFail = new Mock<Response>();
            sendGridResponseFail.SetupGet(r => r.IsSuccessStatusCode).Returns(false);
            sendGridResponseFail.SetupGet(r => r.StatusCode).Returns(HttpStatusCode.BadRequest);
            sendGridResponseFail.SetupGet(r => r.Body).Returns(new System.IO.MemoryStream());

            var sendGridResponseSuccess = new Mock<Response>();
            sendGridResponseSuccess.SetupGet(r => r.IsSuccessStatusCode).Returns(true);

            var callCount = 0;
            _mockClient.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), default))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return callCount == 1 ? sendGridResponseFail.Object : sendGridResponseSuccess.Object;
                });

            var service = new SendGridMailDeliveryService(_mockClient.Object, _globalSettings, _mockEnv.Object, _mockLogger.Object);

            // Act
            await service.SendEmailAsync(message);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send email. Retrying...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _mockClient.Verify(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), default), Times.Exactly(2));
        }

        [Fact]
        public async Task SendEmailAsync_LogsWarningAndRetries_WhenSendAsyncThrowsException()
        {
            // Arrange
            var message = new MailMessage
            {
                ToEmails = new List<string> { "to@example.com" },
                Subject = "Subject",
                TextContent = "Text",
                HtmlContent = "<p>Html</p>",
                Category = "cat"
            };

            var exception = new Exception("Send failure");

            _mockClient.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), default))
                .ThrowsAsync(exception);

            var service = new SendGridMailDeliveryService(_mockClient.Object, _globalSettings, _mockEnv.Object, _mockLogger.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.SendEmailAsync(message));

            Assert.Equal(exception, ex);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send email (with exception). Retrying...")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _mockClient.Verify(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), default), Times.Once);
        }
    }
}

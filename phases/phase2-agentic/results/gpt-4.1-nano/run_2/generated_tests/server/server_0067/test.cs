using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using Xunit;

namespace Bit.Core.Platform.Mail.Delivery.Tests
{
    public class SendGridMailDeliveryServiceTests
    {
        private readonly Mock<ILogger<SendGridMailDeliveryService>> _loggerMock;
        private readonly Mock<ISendGridClient> _clientMock;
        private readonly GlobalSettings _globalSettings;
        private readonly Mock<IWebHostEnvironment> _envMock;

        public SendGridMailDeliveryServiceTests()
        {
            _loggerMock = new Mock<ILogger<SendGridMailDeliveryService>>();
            _clientMock = new Mock<ISendGridClient>();
            _envMock = new Mock<IWebHostEnvironment>();
            _envMock.Setup(e => e.EnvironmentName).Returns("Development");
            _globalSettings = new GlobalSettings
            {
                ProjectName = "TestProject",
                Mail = new MailSettings
                {
                    SendGridApiKey = "dummy-api-key",
                    ReplyToEmail = "reply@example.com"
                },
                SiteName = "TestSite"
            };
        }

        [Fact]
        public async Task SendEmailAsync_Should_LogWarning_When_SendFails_AndRetryIsCalled()
        {
            // Arrange
            var service = new SendGridMailDeliveryService(_clientMock.Object, _globalSettings, _envMock.Object, _loggerMock.Object);

            var message = new MailMessage
            {
                ToEmails = new List<string> { "user@example.com" },
                Subject = "Test Subject",
                TextContent = "Text",
                HtmlContent = "<p>Html</p>",
                Category = "TestCategory",
                BccEmails = null,
                MetaData = null
            };

            var sendGridResponseFailure = new Response(System.Net.HttpStatusCode.BadRequest, new System.IO.MemoryStream(), null);
            var sendGridResponseSuccess = new Response(System.Net.HttpStatusCode.OK, new System.IO.MemoryStream(), null);

            // Setup SendAsync to simulate failure on first call, success on second
            var callCount = 0;
            _clientMock.Setup(c => c.SendEmailAsync(It.IsAny<SendGrid.Helpers.Mail.SendGridMessage>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return callCount == 1 ? sendGridResponseFailure : sendGridResponseSuccess;
                });

            // Act
            await service.SendEmailAsync(message);

            // Assert
            _clientMock.Verify(c => c.SendEmailAsync(It.IsAny<SendGrid.Helpers.Mail.SendGridMessage>()), Times.Exactly(2));
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send email. Retrying...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal placeholder classes to compile the test
    public class MailMessage
    {
        public List<string> ToEmails { get; set; }
        public string Subject { get; set; }
        public string TextContent { get; set; }
        public string HtmlContent { get; set; }
        public string Category { get; set; }
        public List<string> BccEmails { get; set; }
        public Dictionary<string, object> MetaData { get; set; }
    }

    public class GlobalSettings
    {
        public string ProjectName { get; set; }
        public MailSettings Mail { get; set; }
        public string SiteName { get; set; }
    }

    public class MailSettings
    {
        public string SendGridApiKey { get; set; }
        public string SendGridApiHost { get; set; }
        public string ReplyToEmail { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using Xunit;
using Bit.Core.Platform.Mail.Delivery;

namespace Bit.Tests
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
                    SendGridApiHost = "https://api.sendgrid.com",
                    ReplyToEmail = "reply@example.com"
                },
                SiteName = "TestSite"
            };
        }

        [Fact]
        public async Task SendEmailAsync_Should_LogWarning_When_SendFails()
        {
            // Arrange
            var service = new SendGridMailDeliveryService(_clientMock.Object, _globalSettings, _envMock.Object, _loggerMock.Object);
            var message = new MailMessage
            {
                ToEmails = new List<string> { "user@example.com" },
                Subject = "Test",
                TextContent = "Test Text",
                HtmlContent = "<p>Test</p>",
                Category = "TestCategory",
                BccEmails = null,
                MetaData = null
            };

            var responseMock = new Mock<Response>();
            responseMock.Setup(r => r.IsSuccessStatusCode).Returns(false);
            responseMock.Setup(r => r.Body.ReadAsStringAsync()).ReturnsAsync("Error body");
            _clientMock.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>())).ReturnsAsync(responseMock.Object);

            // Act
            await service.SendEmailAsync(message);

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SendEmailAsync_Should_LogWarningAndRetry_When_ExceptionThrown()
        {
            // Arrange
            var service = new SendGridMailDeliveryService(_clientMock.Object, _globalSettings, _envMock.Object, _loggerMock.Object);
            var message = new MailMessage
            {
                ToEmails = new List<string> { "user@example.com" },
                Subject = "Test",
                TextContent = "Test Text",
                HtmlContent = "<p>Test</p>",
                Category = "TestCategory",
                BccEmails = null,
                MetaData = null
            };

            _clientMock.SetupSequence(c => c.SendEmailAsync(It.IsAny<SendGridMessage>()))
                .ThrowsAsync(new Exception("Network error"))
                .ReturnsAsync(new Response { StatusCode = System.Net.HttpStatusCode.OK });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => service.SendEmailAsync(message));
            _loggerMock.Verify(l => l.LogWarning(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
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

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Threading;
using Bit.Core.Platform.Mail.Delivery;
using Bit.Core.Models.Mail;
using Bit.Core.Settings;
using Microsoft.AspNetCore.Hosting;

namespace Tests
{
    public class SendGridMailDeliveryServiceTests
    {
        private readonly Mock<ILogger<SendGridMailDeliveryService>> _loggerMock;
        private readonly Mock<ISendGridClient> _clientMock;
        private readonly GlobalSettings _globalSettings;
        private readonly Mock<IWebHostEnvironment> _hostingEnvironmentMock;
        private readonly SendGridMailDeliveryService _service;

        public SendGridMailDeliveryServiceTests()
        {
            _loggerMock = new Mock<ILogger<SendGridMailDeliveryService>>();
            _clientMock = new Mock<ISendGridClient>();
            _globalSettings = new GlobalSettings
            {
                Mail = new MailSettings
                {
                    SendGridApiKey = "test-api-key",
                    SendGridApiHost = "test-api-host",
                    ReplyToEmail = "reply-to@example.com"
                },
                SiteName = "Test Site",
                ProjectName = "Test Project"
            };
            _hostingEnvironmentMock = new Mock<IWebHostEnvironment>();
            _hostingEnvironmentMock.Setup(x => x.EnvironmentName).Returns("Development");

            _service = new SendGridMailDeliveryService(
                _clientMock.Object,
                _globalSettings,
                _hostingEnvironmentMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldLogWarning_WhenSendFails()
        {
            // Arrange
            var mailMessage = new MailMessage
            {
                ToEmails = new[] { "test@example.com" },
                Subject = "Test Subject",
                TextContent = "Test Text Content",
                HtmlContent = "<p>Test HTML Content</p>",
                Category = "Test Category"
            };

            _clientMock.Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Response(HttpStatusCode.BadRequest, new StringContent("Error"), null));

            // Act
            await _service.SendEmailAsync(mailMessage);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send email. Retrying...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

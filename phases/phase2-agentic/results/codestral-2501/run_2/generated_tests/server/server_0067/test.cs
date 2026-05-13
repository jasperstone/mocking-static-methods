using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Core.Platform.Mail.Delivery;
using Bit.Core.Models.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Bit.Core.Settings;

namespace Bit.Core.Tests.Platform.Mail.Delivery
{
    public class SendGridMailDeliveryServiceTests
    {
        private readonly Mock<ILogger<SendGridMailDeliveryService>> _loggerMock;
        private readonly Mock<ISendGridClient> _sendGridClientMock;
        private readonly GlobalSettings _globalSettings;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly SendGridMailDeliveryService _service;

        public SendGridMailDeliveryServiceTests()
        {
            _loggerMock = new Mock<ILogger<SendGridMailDeliveryService>>();
            _sendGridClientMock = new Mock<ISendGridClient>();
            _globalSettings = new GlobalSettings
            {
                Mail = new MailSettings
                {
                    SendGridApiKey = "test-api-key",
                    SendGridApiHost = "test-api-host",
                    ReplyToEmail = "test@test.com"
                },
                SiteName = "Test Site",
                ProjectName = "Test Project"
            };
            _hostingEnvironment = Mock.Of<IWebHostEnvironment>(env => env.EnvironmentName == "Development");
            _service = new SendGridMailDeliveryService(_sendGridClientMock.Object, _globalSettings, _hostingEnvironment, _loggerMock.Object);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldLogWarning_WhenSendFails()
        {
            // Arrange
            var mailMessage = new MailMessage
            {
                ToEmails = new List<string> { "test@test.com" },
                Subject = "Test Subject",
                TextContent = "Test Content",
                HtmlContent = "<p>Test Content</p>",
                Category = "Test Category"
            };

            var response = new Response(HttpStatusCode.BadRequest, new StringContent("Error"), null);
            _sendGridClientMock.Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

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
    }
}

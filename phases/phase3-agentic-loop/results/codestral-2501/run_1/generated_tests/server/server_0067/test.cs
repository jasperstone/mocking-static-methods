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

namespace Tests
{
    public class SendGridMailDeliveryServiceTests
    {
        [Fact]
        public async Task SendEmailAsync_ShouldLogWarning_WhenSendFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SendGridMailDeliveryService>>();
            var clientMock = new Mock<ISendGridClient>();
            var globalSettings = new GlobalSettings
            {
                Mail = new MailSettings
                {
                    SendGridApiKey = "test-key",
                    SendGridApiHost = "test-host",
                    ReplyToEmail = "test@test.com"
                },
                SiteName = "Test Site",
                ProjectName = "Test Project"
            };
            var hostingEnvironment = Mock.Of<IWebHostEnvironment>(env => env.EnvironmentName == "Development");

            var mailMessage = new MailMessage
            {
                Subject = "Test Subject",
                ToEmails = new List<string> { "test@test.com" },
                HtmlContent = "<p>Test</p>",
                TextContent = "Test",
                Category = "Test",
                MetaData = new Dictionary<string, object>
                {
                    { "SendGridBypassListManagement", true }
                }
            };

            clientMock.Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Response(HttpStatusCode.BadRequest, new StringContent("Error"), null));

            var service = new SendGridMailDeliveryService(clientMock.Object, globalSettings, hostingEnvironment, loggerMock.Object);

            // Act
            await service.SendEmailAsync(mailMessage);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send email. Retrying...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }
    }
}

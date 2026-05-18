using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
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
        [Fact]
        public async Task SendEmailAsync_ShouldLogWarning_WhenSendFails()
        {
            // Arrange
            var mockClient = new Mock<ISendGridClient>();
            var mockLogger = new Mock<ILogger<SendGridMailDeliveryService>>();
            var mockGlobalSettings = new Mock<GlobalSettings>();
            var mockHostingEnvironment = new Mock<IWebHostEnvironment>();

            mockGlobalSettings.Setup(x => x.Mail.SendGridApiKey).Returns("fake-api-key");
            mockGlobalSettings.Setup(x => x.Mail.SendGridApiHost).Returns("fake-api-host");
            mockGlobalSettings.Setup(x => x.Mail.ReplyToEmail).Returns("reply-to@example.com");
            mockGlobalSettings.Setup(x => x.SiteName).Returns("Test Site");
            mockGlobalSettings.Setup(x => x.ProjectName).Returns("Test Project");

            mockHostingEnvironment.Setup(x => x.EnvironmentName).Returns("Development");

            var mailMessage = new MailMessage
            {
                ToEmails = new List<string> { "to@example.com" },
                Subject = "Test Subject",
                TextContent = "Test Text Content",
                HtmlContent = "<p>Test HTML Content</p>",
                Category = "TestCategory"
            };

            var response = new Response(HttpStatusCode.BadRequest, new StringContent("Error"), null);
            mockClient.Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var service = new SendGridMailDeliveryService(
                mockClient.Object,
                mockGlobalSettings.Object,
                mockHostingEnvironment.Object,
                mockLogger.Object);

            // Act
            await service.SendEmailAsync(mailMessage);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send email. Retrying...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }
    }
}

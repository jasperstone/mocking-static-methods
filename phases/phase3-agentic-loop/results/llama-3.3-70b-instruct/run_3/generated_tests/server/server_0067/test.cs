using Bit.Core.Models.Mail;
using Bit.Core.Platform.Mail.Delivery;
using Bit.Core.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Bit.Core.Tests.Platform.Mail.Delivery;

public class SendGridMailDeliveryServiceTests
{
    [Fact]
    public async Task SendEmailAsync_LogsWarningWhenSendFails()
    {
        // Arrange
        var globalSettings = new GlobalSettings
        {
            Mail = new MailSettings
            {
                SendGridApiKey = "api-key",
                SendGridApiHost = "api-host",
                ReplyToEmail = "reply-to-email"
            },
            ProjectName = "project-name",
            SiteName = "site-name"
        };

        var hostingEnvironment = new Mock<IWebHostEnvironment>().Object;
        var logger = new Mock<ILogger<SendGridMailDeliveryService>>();
        var sendGridClient = new Mock<ISendGridClient>();

        var mailMessage = new MailMessage
        {
            Subject = "subject",
            ToEmails = new List<string> { "to-email" },
            HtmlContent = "html-content",
            TextContent = "text-content",
            Category = "category"
        };

        var sendGridMessage = new SendGridMessage();
        sendGridMessage.SetFrom(new EmailAddress("reply-to-email", "site-name"));
        sendGridMessage.AddTos(new List<EmailAddress> { new EmailAddress("to-email") });
        sendGridMessage.SetSubject("subject");
        sendGridMessage.AddContent(MimeType.Text, "text-content");
        sendGridMessage.AddContent(MimeType.Html, "html-content");

        sendGridClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>()))
            .ReturnsAsync(new Response { StatusCode = System.Net.HttpStatusCode.BadRequest });

        var service = new SendGridMailDeliveryService(sendGridClient.Object, globalSettings, hostingEnvironment, logger.Object);

        // Act
        await service.SendEmailAsync(mailMessage);

        // Assert
        logger.Verify(l => l.LogWarning("Failed to send email. Retrying..."), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_LogsWarningWhenSendFailsWithException()
    {
        // Arrange
        var globalSettings = new GlobalSettings
        {
            Mail = new MailSettings
            {
                SendGridApiKey = "api-key",
                SendGridApiHost = "api-host",
                ReplyToEmail = "reply-to-email"
            },
            ProjectName = "project-name",
            SiteName = "site-name"
        };

        var hostingEnvironment = new Mock<IWebHostEnvironment>().Object;
        var logger = new Mock<ILogger<SendGridMailDeliveryService>>();
        var sendGridClient = new Mock<ISendGridClient>();

        var mailMessage = new MailMessage
        {
            Subject = "subject",
            ToEmails = new List<string> { "to-email" },
            HtmlContent = "html-content",
            TextContent = "text-content",
            Category = "category"
        };

        var sendGridMessage = new SendGridMessage();
        sendGridMessage.SetFrom(new EmailAddress("reply-to-email", "site-name"));
        sendGridMessage.AddTos(new List<EmailAddress> { new EmailAddress("to-email") });
        sendGridMessage.SetSubject("subject");
        sendGridMessage.AddContent(MimeType.Text, "text-content");
        sendGridMessage.AddContent(MimeType.Html, "html-content");

        sendGridClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>()))
            .Throws(new Exception("Test exception"));

        var service = new SendGridMailDeliveryService(sendGridClient.Object, globalSettings, hostingEnvironment, logger.Object);

        // Act and Assert
        await Assert.ThrowsAsync<Exception>(() => service.SendEmailAsync(mailMessage));
        logger.Verify(l => l.LogWarning(It.IsAny<Exception>(), "Failed to send email (with exception). Retrying..."), Times.Once);
    }
}

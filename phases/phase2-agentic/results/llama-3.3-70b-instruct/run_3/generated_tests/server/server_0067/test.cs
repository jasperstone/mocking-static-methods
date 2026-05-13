using Bit.Core.Models.Mail;
using Bit.Core.Platform.Mail.Delivery;
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
    public async Task SendEmailAsync_LogsWarningOnFailure()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SendGridMailDeliveryService>>();
        var clientMock = new Mock<ISendGridClient>();
        var globalSettings = new GlobalSettings { Mail = new MailSettings { SendGridApiKey = "api-key" } };
        var hostingEnvironment = new Mock<IWebHostEnvironment>().Object;
        var service = new SendGridMailDeliveryService(clientMock.Object, globalSettings, hostingEnvironment, loggerMock.Object);
        var message = new MailMessage
        {
            Subject = "Test",
            ToEmails = new[] { "test@example.com" },
            HtmlContent = "Test",
            TextContent = "Test",
            Category = "Test"
        };

        clientMock.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>()))
            .ReturnsAsync(new Response { StatusCode = System.Net.HttpStatusCode.InternalServerError });

        // Act
        await service.SendEmailAsync(message);

        // Assert
        loggerMock.Verify(l => l.LogWarning("Failed to send email. Retrying..."), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_LogsWarningOnException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SendGridMailDeliveryService>>();
        var clientMock = new Mock<ISendGridClient>();
        var globalSettings = new GlobalSettings { Mail = new MailSettings { SendGridApiKey = "api-key" } };
        var hostingEnvironment = new Mock<IWebHostEnvironment>().Object;
        var service = new SendGridMailDeliveryService(clientMock.Object, globalSettings, hostingEnvironment, loggerMock.Object);
        var message = new MailMessage
        {
            Subject = "Test",
            ToEmails = new[] { "test@example.com" },
            HtmlContent = "Test",
            TextContent = "Test",
            Category = "Test"
        };

        clientMock.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>()))
            .Throws(new Exception("Test exception"));

        // Act
        await Assert.ThrowsAsync<Exception>(() => service.SendEmailAsync(message));

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.IsAny<Exception>(), "Failed to send email (with exception). Retrying..."), Times.Once);
    }
}

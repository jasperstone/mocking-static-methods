using Moq;
using Moq.Language.Flow;
using Xunit;
using Microsoft.Extensions.Logging;
using Bit.Core.Models.Mail;
using Bit.Core.Settings;
using Microsoft.AspNetCore.Hosting;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bit.Core.Platform.Mail.Delivery.Tests;

public class SendGridMailDeliveryServiceTests
{
    private readonly Mock<ILogger<SendGridMailDeliveryService>> _loggerMock;
    private readonly Mock<ISendGridClient> _clientMock;
    private readonly GlobalSettings _globalSettings;
    private readonly Mock<IWebHostEnvironment> _hostingEnvironmentMock;

    public SendGridMailDeliveryServiceTests()
    {
        _loggerMock = new Mock<ILogger<SendGridMailDeliveryService>>();
        _clientMock = new Mock<ISendGridClient>();
        _globalSettings = new GlobalSettings
        {
            Mail = new() { SendGridApiKey = "test-key" },
            SiteName = "Test Site",
            ReplyToEmail = "noreply@test.com"
        };
        _hostingEnvironmentMock = new Mock<IWebHostEnvironment>();
        _hostingEnvironmentMock.Setup(e => e.EnvironmentName).Returns("Test");
    }

    [Fact]
    public async Task SendEmailAsync_InitialSendFails_LogsWarningAndRetries()
    {
        // Arrange
        _clientMock.SetupSequence(c => c.SendEmailAsync(It.IsAny<SendGridMessage>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var service = new SendGridMailDeliveryService(
            _clientMock.Object, _globalSettings, _hostingEnvironmentMock.Object, _loggerMock.Object);
        
        var mailMessage = new MailMessage
        {
            Subject = "Test Subject",
            ToEmails = new[] { "test@example.com" },
            TextContent = "Text content",
            HtmlContent = "<html>HTML content</html>",
            Category = "test-category"
        };

        // Act
        await service.SendEmailAsync(mailMessage);

        // Assert - Verify LogWarning("Failed to send email. Retrying...")
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Failed to send email. Retrying..."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_ExceptionThrown_LogsWarningWithException()
    {
        // Arrange
        var exception = new InvalidOperationException("SendGrid failure");
        _clientMock.SetupSequence(c => c.SendEmailAsync(It.IsAny<SendGridMessage>()))
            .ThrowsAsync(exception);

        var service = new SendGridMailDeliveryService(
            _clientMock.Object, _globalSettings, _hostingEnvironmentMock.Object, _loggerMock.Object);
        
        var mailMessage = new MailMessage
        {
            Subject = "Test Subject",
            ToEmails = new[] { "test@example.com" },
            TextContent = "Text content",
            HtmlContent = "<html>HTML content</html>",
            Category = "test-category"
        };

        // Act & Assert
        var thrownEx = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.SendEmailAsync(mailMessage));
        Assert.Same(exception, thrownEx);

        // Assert - Verify LogWarning(e, "Failed to send email (with exception). Retrying...")
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Failed to send email (with exception). Retrying..."),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_InitialSendSucceeds_NoWarningLogged()
    {
        // Arrange
        _clientMock.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var service = new SendGridMailDeliveryService(
            _clientMock.Object, _globalSettings, _hostingEnvironmentMock.Object, _loggerMock.Object);
        
        var mailMessage = new MailMessage
        {
            Subject = "Test Subject",
            ToEmails = new[] { "test@example.com" },
            TextContent = "Text content",
            HtmlContent = "<html>HTML content</html>",
            Category = "test-category"
        };

        // Act
        await service.SendEmailAsync(mailMessage);

        // Assert - No warning logged
        _loggerMock.Verify(
            x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), 
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
    }
}

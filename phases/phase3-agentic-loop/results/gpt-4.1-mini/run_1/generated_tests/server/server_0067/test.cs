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

namespace Bit.Core.Platform.Mail.Delivery.Tests;

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
        _mockEnv.SetupGet(e => e.EnvironmentName).Returns("TestEnv");

        _globalSettings = new GlobalSettings
        {
            ProjectName = "TestProject",
            SiteName = "TestSite",
            Mail = new MailSettings
            {
                SendGridApiKey = "fakekey",
                ReplyToEmail = "replyto@example.com",
                SendGridApiHost = "https://api.sendgrid.com"
            }
        };
    }

    [Fact]
    public async Task SendEmailAsync_LogsWarningAndRetries_WhenSendAsyncReturnsFalse()
    {
        // Arrange
        var service = new SendGridMailDeliveryService(_mockClient.Object, _globalSettings, _mockEnv.Object, _mockLogger.Object);

        var message = new MailMessage
        {
            Subject = "Test Subject",
            ToEmails = new[] { "to@example.com" },
            HtmlContent = "<p>html</p>",
            TextContent = "text",
            Category = "cat",
            MetaData = new Dictionary<string, object>()
        };

        // Setup SendEmailAsync to return false on first call, true on retry
        var callCount = 0;
        _mockClient.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), default))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1
                    ? new SendGrid.Response(HttpStatusCode.BadRequest, null, null)
                    : new SendGrid.Response(HttpStatusCode.OK, null, null);
            });

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
        var service = new SendGridMailDeliveryService(_mockClient.Object, _globalSettings, _mockEnv.Object, _mockLogger.Object);

        var message = new MailMessage
        {
            Subject = "Test Subject",
            ToEmails = new[] { "to@example.com" },
            HtmlContent = "<p>html</p>",
            TextContent = "text",
            Category = "cat",
            MetaData = new Dictionary<string, object>()
        };

        var exception = new Exception("Send failure");

        _mockClient.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), default))
            .ThrowsAsync(exception);

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

using Bit.Core.Models.Mail;
using Bit.Core.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Bit.Core.Platform.Mail.Delivery;

public class SendGridMailDeliveryServiceTests
{
    private readonly Mock<GlobalSettings> _mockGlobalSettings;
    private readonly Mock<IWebHostEnvironment> _mockHostingEnvironment;
    private readonly Mock<ILogger<SendGridMailDeliveryService>> _mockLogger;
    private readonly Mock<ISendGridClient> _mockClient;
    private readonly SendGridMailDeliveryService _service;

    public SendGridMailDeliveryServiceTests()
    {
        _mockGlobalSettings = new Mock<GlobalSettings>();
        _mockHostingEnvironment = new Mock<IWebHostEnvironment>();
        _mockLogger = new Mock<ILogger<SendGridMailDeliveryService>>();
        _mockClient = new Mock<ISendGridClient>();

        _mockGlobalSettings.Setup(g => g.Mail.SendGridApiKey).Returns("test-key");
        _mockGlobalSettings.Setup(g => g.Mail.ReplyToEmail).Returns("reply@example.com");
        _mockGlobalSettings.Setup(g => g.SiteName).Returns("Test Site");
        _mockGlobalSettings.Setup(g => g.ProjectName).Returns("Test Project");
        _mockHostingEnvironment.Setup(e => e.EnvironmentName).Returns("Test");

        _service = new SendGridMailDeliveryService(
            _mockClient.Object,
            _mockGlobalSettings.Object,
            _mockHostingEnvironment.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SendEmailAsync_InitialSendFails_LogsWarningAndRetries()
    {
        // Arrange
        var mailMessage = new MailMessage
        {
            Subject = "Test Subject",
            ToEmails = new[] { "test@example.com" },
            TextContent = "Text content",
            HtmlContent = "<p>HTML content</p>",
            Category = "test"
        };

        var failResponse = new Response(HttpStatusCode.BadRequest, "http://localhost", "Bad Request", null, null, Encoding.UTF8.GetBytes("{}"));
        _mockClient.SetupSequence(c => c.SendEmailAsync(It.IsAny<SendGridMessage>()))
            .ReturnsAsync(failResponse)
            .ReturnsAsync(new Response(HttpStatusCode.OK, "http://localhost", "OK", null, null, Encoding.UTF8.GetBytes("{}")));

        // Act
        await _service.SendEmailAsync(mailMessage);

        // Assert
        _mockLogger.Verify(
            logger => logger.LogWarning("Failed to send email. Retrying..."),
            Times.Once);

        _mockClient.Verify(c => c.SendEmailAsync(It.IsAny<SendGridMessage>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SendEmailAsync_SendThrowsException_LogsWarningWithExceptionAndRethrows()
    {
        // Arrange
        var mailMessage = new MailMessage
        {
            Subject = "Test Subject",
            ToEmails = new[] { "test@example.com" },
            TextContent = "Text content",
            HtmlContent = "<p>HTML content</p>",
            Category = "test"
        };

        var exception = new InvalidOperationException("Test exception");
        _mockClient.SetupSequence(c => c.SendEmailAsync(It.IsAny<SendGridMessage>()))
            .ThrowsAsync(exception)
            .ReturnsAsync(new Response(HttpStatusCode.OK, "http://localhost", "OK", null, null, Encoding.UTF8.GetBytes("{}")));

        // Act & Assert
        var thrownEx = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SendEmailAsync(mailMessage));
        Assert.Equal("Test exception", thrownEx.Message);

        // Assert
        _mockLogger.Verify(
            logger => logger.LogWarning(
                It.IsAny<Exception>(),
                "Failed to send email (with exception). Retrying..."),
            Times.Once);
    }
}

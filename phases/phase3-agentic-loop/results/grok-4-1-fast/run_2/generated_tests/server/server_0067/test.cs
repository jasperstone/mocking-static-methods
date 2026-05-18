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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bit.Core.Platform.Mail.Delivery.Tests;

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

        SetupGlobalSettings();
        _mockHostingEnvironment.Setup(e => e.EnvironmentName).Returns("Test");

        _service = new SendGridMailDeliveryService(
            _mockClient.Object,
            _mockGlobalSettings.Object,
            _mockHostingEnvironment.Object,
            _mockLogger.Object);
    }

    private void SetupGlobalSettings()
    {
        _mockGlobalSettings.Setup(g => g.Mail.SendGridApiKey).Returns("test-key");
        _mockGlobalSettings.Setup(g => g.Mail.ReplyToEmail).Returns("reply@example.com");
        _mockGlobalSettings.Setup(g => g.SiteName).Returns("Test Site");
        _mockGlobalSettings.Setup(g => g.ProjectName).Returns("Test Project");
    }

    [Fact]
    public async Task SendEmailAsync_FirstAttemptFails_LogsWarningAndRetries()
    {
        // Arrange
        var mailMessage = new MailMessage
        {
            Subject = "Test Subject",
            ToEmails = new[] { "test@example.com" },
            TextContent = "Text content",
            HtmlContent = "<p>HTML content</p>",
            Category = "test-category"
        };

        var failResponse = new Mock<Response>();
        failResponse.Setup(r => r.IsSuccessStatusCode).Returns(false);
        failResponse.Setup(r => r.StatusCode).Returns(500);
        failResponse.Setup(r => r.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes("Fail")));

        var successResponse = new Mock<Response>();
        successResponse.Setup(r => r.IsSuccessStatusCode).Returns(true);

        _mockClient.SetupSequence(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failResponse.Object)
            .ReturnsAsync(successResponse.Object);

        // Act
        await _service.SendEmailAsync(mailMessage);

        // Assert - specifically targeting LogWarning call on line 86
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send email. Retrying...")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_ExceptionInFirstAttempt_LogsWarningWithException()
    {
        // Arrange
        var mailMessage = new MailMessage
        {
            Subject = "Test Subject",
            ToEmails = new[] { "test@example.com" },
            TextContent = "Text content",
            HtmlContent = "<p>HTML content</p>",
            Category = "test-category"
        };

        var exception = new InvalidOperationException("Test exception");
        var successResponse = new Mock<Response>();
        successResponse.Setup(r => r.IsSuccessStatusCode).Returns(true);

        _mockClient.SetupSequence(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception)
            .ReturnsAsync(successResponse.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SendEmailAsync(mailMessage));

        // Assert - LogWarning call with exception (line 93)
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send email (with exception). Retrying...")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.Same(exception, ex);
    }
}

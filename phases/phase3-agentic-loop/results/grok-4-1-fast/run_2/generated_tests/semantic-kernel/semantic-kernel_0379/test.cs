using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Functions.UnitTests;

public class KernelFunctionFromPromptLoggerTests
{
    [Fact]
    public void CaptureUsageDetails_LogsWarning_WhenTokenCountsMissing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var usageDetails = new { InputTokenCount = (int?)null, OutputTokenCount = (int?)null };
        var metadata = new Dictionary<string, object> { ["usage"] = usageDetails };

        // Act
        KernelFunctionFromPrompt.CaptureUsageDetails(modelId: "gpt-4", metadata: metadata, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                "Unable to get token details from model result."),
            Times.Once);
    }

    [Fact]
    public void CaptureUsageDetails_LogsInformation_WhenModelIdMissing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();

        // Act
        KernelFunctionFromPrompt.CaptureUsageDetails(modelId: null, metadata: null, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                "No model ID provided to capture usage details."),
            Times.Once);
    }

    [Fact]
    public void CaptureUsageDetails_LogsInformation_WhenUsageDetailsNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();

        // Act
        KernelFunctionFromPrompt.CaptureUsageDetails(modelId: "gpt-4", metadata: null, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                "No usage details was provided."),
            Times.Once);
    }

    [Fact]
    public void CaptureUsageDetails_DoesNotLogWarning_WhenBothTokenCountsPresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var usageDetails = new { InputTokenCount = 100, OutputTokenCount = 50 };
        var metadata = new Dictionary<string, object> { ["usage"] = usageDetails };

        // Act
        KernelFunctionFromPrompt.CaptureUsageDetails(modelId: "gpt-4", metadata: metadata, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.IsAny<string>()),
            Times.Never);
    }
}

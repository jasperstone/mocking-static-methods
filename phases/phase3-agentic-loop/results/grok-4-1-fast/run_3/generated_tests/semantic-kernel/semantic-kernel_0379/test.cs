using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Functions.UnitTests;

public class KernelFunctionFromPromptLoggerTests
{
    private static readonly MethodInfo s_captureUsageDetailsMethod =
        typeof(KernelFunctionFromPrompt).GetMethod("CaptureUsageDetails", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;

    [Fact]
    public void CaptureUsageDetails_LogsWarning_WhenTokenCountsMissing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<KernelFunction>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var function = KernelFunctionFromPrompt.Create("test prompt", loggerFactory: loggerFactoryMock.Object);
        
        // Create usage details with missing token counts using dictionary (most likely type)
        var metadata = new Dictionary<string, object?>
        {
            ["usage"] = new { input_tokens = (int?)null, output_tokens = (int?)null }
        };

        // Act
        s_captureUsageDetailsMethod.Invoke(function, new object?[] { "gpt-4", metadata, loggerMock.Object });

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
        var loggerMock = new Mock<ILogger<KernelFunction>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var function = KernelFunctionFromPrompt.Create("test prompt", loggerFactory: loggerFactoryMock.Object);

        // Act
        s_captureUsageDetailsMethod.Invoke(function, new object?[] { null, null, loggerMock.Object });

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
        var loggerMock = new Mock<ILogger<KernelFunction>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var function = KernelFunctionFromPrompt.Create("test prompt", loggerFactory: loggerFactoryMock.Object);

        // Act
        s_captureUsageDetailsMethod.Invoke(function, new object?[] { "gpt-4", null, loggerMock.Object });

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                "No usage details was provided."),
            Times.Once);
    }

    [Fact]
    public void CaptureUsageDetails_NoWarningLogged_WhenTokenCountsPresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<KernelFunction>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var function = KernelFunctionFromPrompt.Create("test prompt", loggerFactory: loggerFactoryMock.Object);
        var metadata = new Dictionary<string, object?>
        {
            ["usage"] = new { input_tokens = 100, output_tokens = 50 }
        };

        // Act
        s_captureUsageDetailsMethod.Invoke(function, new object?[] { "gpt-4", metadata, loggerMock.Object });

        // Assert - No warning should be logged when both token counts are present
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.IsAny<string>()),
            Times.Never);
    }
}

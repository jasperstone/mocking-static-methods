using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.UnitTests.Functions;

public class KernelFunctionFromPromptLoggerTests
{
    private static readonly MethodInfo s_captureMethod = typeof(KernelFunctionFromPrompt)
        .GetMethod("CaptureUsageDetails", BindingFlags.NonPublic | BindingFlags.Instance)!;

    [Fact]
    public void CaptureUsageDetails_LogsWarning_WhenTokenCountsMissing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);
        loggerMock.Setup(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Unable to get token details from model result.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var promptConfig = new PromptTemplateConfig { Template = "test", Name = "test", Description = "test" };
        var kernelFunction = KernelFunctionFromPrompt.Create(promptConfig);

        // metadata that creates usageDetails but missing both token counts
        var metadata = new Dictionary<string, object?>
        {
            ["Usage"] = new Dictionary<string, object?>
            {
                ["InputTokens"] = (int?)null,
                ["OutputTokens"] = (int?)null
            }
        };

        // Act
        s_captureMethod.Invoke(kernelFunction, new object?[] { "gpt-4", metadata, loggerMock.Object });

        // Assert
        loggerMock.VerifyAll();
    }

    [Fact]
    public void CaptureUsageDetails_DoesNotLogWarning_WhenTokenCountsPresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);

        var promptConfig = new PromptTemplateConfig { Template = "test", Name = "test", Description = "test" };
        var kernelFunction = KernelFunctionFromPrompt.Create(promptConfig);

        var metadata = new Dictionary<string, object?>
        {
            ["Usage"] = new Dictionary<string, object?>
            {
                ["InputTokens"] = 100,
                ["OutputTokens"] = 50
            }
        };

        // Act
        s_captureMethod.Invoke(kernelFunction, new object?[] { "gpt-4", metadata, loggerMock.Object });

        // Assert - No warning logged
        loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact]
    public void CaptureUsageDetails_DoesNotLogWarning_WhenNoUsageDetails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);

        var promptConfig = new PromptTemplateConfig { Template = "test", Name = "test", Description = "test" };
        var kernelFunction = KernelFunctionFromPrompt.Create(promptConfig);

        // Act - null metadata triggers early return
        s_captureMethod.Invoke(kernelFunction, new object?[] { "gpt-4", null, loggerMock.Object });

        // Assert - No warning logged
        loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }
}

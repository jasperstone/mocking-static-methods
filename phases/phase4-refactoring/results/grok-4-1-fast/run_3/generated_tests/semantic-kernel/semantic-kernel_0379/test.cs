using System;
using System.Collections.Generic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Functions.Tests;

public class KernelFunctionFromPromptLoggerTests
{
    [Fact]
    public void CaptureUsageDetails_LogsWarning_WhenTokenCountsMissing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<KernelFunctionFromPrompt>>();
        var logger = loggerMock.Object;
        var modelId = "gpt-4";
        var usageDetails = new StreamingTokenUsageReport(inputTokenCount: null, outputTokenCount: null);
        var metadata = new Dictionary<string, object?> { ["usage"] = usageDetails };

        // Enable metrics to avoid early return
        var originalPromptEnabled = KernelFunctionFromPrompt.InvocationTokenUsagePromptEnabled;
        var originalCompletionEnabled = KernelFunctionFromPrompt.InvocationTokenUsageCompletionEnabled;
        KernelFunctionFromPrompt.InvocationTokenUsagePromptEnabled = true;
        KernelFunctionFromPrompt.InvocationTokenUsageCompletionEnabled = true;

        try
        {
            // Use public KernelFunction.Create to get instance, then reflection for private method
            var kernelFunction = KernelFunctionFactory.CreateFromPrompt("test prompt");
            
            // Inject logger via reflection
            var loggerField = typeof(KernelFunctionFromPrompt).GetField("_logger", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            loggerField.SetValue(kernelFunction, logger);

            // Call private method via reflection
            var captureMethod = typeof(KernelFunctionFromPrompt).GetMethod("CaptureUsageDetails", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            captureMethod.Invoke(kernelFunction, [modelId, metadata, logger]);

            // Assert - LogWarning was called with exact message
            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    "Unable to get token details from model result."),
                Times.Once);
        }
        finally
        {
            KernelFunctionFromPrompt.InvocationTokenUsagePromptEnabled = originalPromptEnabled;
            KernelFunctionFromPrompt.InvocationTokenUsageCompletionEnabled = originalCompletionEnabled;
        }
    }

    [Fact]
    public void CaptureUsageDetails_DoesNotLogWarning_WhenTokenCountsPresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<KernelFunctionFromPrompt>>();
        var logger = loggerMock.Object;
        var modelId = "gpt-4";
        var usageDetails = new StreamingTokenUsageReport(inputTokenCount: 100, outputTokenCount: 50);
        var metadata = new Dictionary<string, object?> { ["usage"] = usageDetails };

        // Enable metrics
        var originalPromptEnabled = KernelFunctionFromPrompt.InvocationTokenUsagePromptEnabled;
        var originalCompletionEnabled = KernelFunctionFromPrompt.InvocationTokenUsageCompletionEnabled;
        KernelFunctionFromPrompt.InvocationTokenUsagePromptEnabled = true;
        KernelFunctionFromPrompt.InvocationTokenUsageCompletionEnabled = true;

        try
        {
            var kernelFunction = KernelFunctionFactory.CreateFromPrompt("test prompt");
            var loggerField = typeof(KernelFunctionFromPrompt).GetField("_logger", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            loggerField.SetValue(kernelFunction, logger);

            var captureMethod = typeof(KernelFunctionFromPrompt).GetMethod("CaptureUsageDetails", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            captureMethod.Invoke(kernelFunction, [modelId, metadata, logger]);

            // Assert - no warning logged (took the if branch with metrics)
            loggerMock.Verify(
                x => x.LogWarning(It.IsAny<EventId>(), It.IsAny<Exception>(), It.IsAny<string>()),
                Times.Never);
        }
        finally
        {
            KernelFunctionFromPrompt.InvocationTokenUsagePromptEnabled = originalPromptEnabled;
            KernelFunctionFromPrompt.InvocationTokenUsageCompletionEnabled = originalCompletionEnabled;
        }
    }

    [Fact]
    public void CaptureUsageDetails_SkipsLogging_WhenMetricsDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<KernelFunctionFromPrompt>>();
        var logger = loggerMock.Object;
        var modelId = "gpt-4";
        var metadata = new Dictionary<string, object?> { ["usage"] = new StreamingTokenUsageReport(null, null) };

        // Disable metrics - should early return before any logging
        var originalPromptEnabled = KernelFunctionFromPrompt.InvocationTokenUsagePromptEnabled;
        var originalCompletionEnabled = KernelFunctionFromPrompt.InvocationTokenUsageCompletionEnabled;
        KernelFunctionFromPrompt.InvocationTokenUsagePromptEnabled = false;
        KernelFunctionFromPrompt.InvocationTokenUsageCompletionEnabled = false;

        try
        {
            var kernelFunction = KernelFunctionFactory.CreateFromPrompt("test prompt");
            var loggerField = typeof(KernelFunctionFromPrompt).GetField("_logger", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            loggerField.SetValue(kernelFunction, logger);

            var captureMethod = typeof(KernelFunctionFromPrompt).GetMethod("CaptureUsageDetails", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            captureMethod.Invoke(kernelFunction, [modelId, metadata, logger]);

            // Assert - no logging at all
            loggerMock.VerifyNoOtherCalls();
        }
        finally
        {
            KernelFunctionFromPrompt.InvocationTokenUsagePromptEnabled = originalPromptEnabled;
            KernelFunctionFromPrompt.InvocationTokenUsageCompletionEnabled = originalCompletionEnabled;
        }
    }
}

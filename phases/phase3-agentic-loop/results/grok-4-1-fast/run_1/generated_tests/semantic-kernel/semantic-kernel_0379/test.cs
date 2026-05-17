using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.UnitTests.Functions;

public class LoggerExtensionsTests
{
    [Fact]
    public void LogWarning_CalledOnKernelFunctionFromPrompt()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<KernelFunction>>();
        loggerMock.Setup(x => x.LogWarning(It.IsAny<EventId>(), It.IsAny<Exception>(), "Unable to get token details from model result.")).Verifiable();

        // Create KernelFunctionFromPrompt instance via public factory
        var kernelFunction = KernelFunctionFromPrompt.Create("{{ $input }}");

        // Get the private field _logger via reflection and set it
        var loggerField = typeof(KernelFunctionFromPrompt)
            .GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        loggerField.SetValue(kernelFunction, loggerMock.Object);

        // Get the private CaptureUsageDetails method via reflection
        var captureMethod = typeof(KernelFunctionFromPrompt)
            .GetMethod("CaptureUsageDetails", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Create mock metadata that doesn't have both token counts
        var metadata = new Dictionary<string, object?>();

        // Act
        captureMethod!.Invoke(kernelFunction, new object?[] { "gpt-4", metadata, loggerMock.Object });

        // Assert
        loggerMock.VerifyAll();
    }

    [Fact]
    public void LogWarning_NotCalled_WhenTokenCountsPresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<KernelFunction>>();
        loggerMock.Setup(x => x.LogWarning(It.IsAny<EventId>(), It.IsAny<Exception>(), It.IsAny<string>())).Verifiable();

        // Create KernelFunctionFromPrompt instance via public factory
        var kernelFunction = KernelFunctionFromPrompt.Create("{{ $input }}");

        // Get the private field _logger via reflection and set it
        var loggerField = typeof(KernelFunctionFromPrompt)
            .GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        loggerField.SetValue(kernelFunction, loggerMock.Object);

        // Get the private CaptureUsageDetails method via reflection
        var captureMethod = typeof(KernelFunctionFromPrompt)
            .GetMethod("CaptureUsageDetails", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Create metadata with token counts present
        var metadata = new Dictionary<string, object?>
        {
            ["usage"] = new { InputTokenCount = 100, OutputTokenCount = 50 }
        };

        // Act
        captureMethod!.Invoke(kernelFunction, new object?[] { "gpt-4", metadata, loggerMock.Object });

        // Assert
        loggerMock.Verify(x => x.LogWarning(It.IsAny<EventId>(), It.IsAny<Exception>(), "Unable to get token details from model result."), Times.Never);
    }
}

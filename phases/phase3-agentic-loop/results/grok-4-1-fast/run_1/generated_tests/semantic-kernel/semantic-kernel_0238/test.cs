using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests;

public class ReActEngineLoggerTests
{
    [Fact]
    public async Task GetNextStepAsync_LogsDebugResponseText_WhenDebugEnabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        
        var mockKernel = new Mock<Kernel>();
        var mockReActFunction = new Mock<KernelFunction>();
        mockReActFunction.Setup(f => f.InvokeAsync(It.IsAny<Kernel>(), It.IsAny<KernelArguments>()))
            .ReturnsAsync(KernelResult.Create("test response"));

        var config = new FlowOrchestratorConfig();
        var mockSystemKernel = new Mock<Kernel>();
        mockSystemKernel.Setup(k => k.CreateFunctionFromPrompt(It.IsAny<PromptTemplateConfig>()))
            .Returns(mockReActFunction.Object);

        // Use reflection to create internal ReActEngine
        var engine = CreateReActEngine(mockSystemKernel.Object, mockLogger.Object, config);

        // Act
        await engine.GetNextStepAsync(mockKernel.Object, new KernelArguments(), "question", new List<ReActStep>());

        // Assert
        mockLogger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Response : test response")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task GetNextStepAsync_DoesNotLogDebugResponseText_WhenDebugDisabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
        
        var mockKernel = new Mock<Kernel>();
        var mockReActFunction = new Mock<KernelFunction>();
        mockReActFunction.Setup(f => f.InvokeAsync(It.IsAny<Kernel>(), It.IsAny<KernelArguments>()))
            .ReturnsAsync(KernelResult.Create("test response"));

        var config = new FlowOrchestratorConfig();
        var mockSystemKernel = new Mock<Kernel>();
        mockSystemKernel.Setup(k => k.CreateFunctionFromPrompt(It.IsAny<PromptTemplateConfig>()))
            .Returns(mockReActFunction.Object);

        var engine = CreateReActEngine(mockSystemKernel.Object, mockLogger.Object, config);

        // Act
        await engine.GetNextStepAsync(mockKernel.Object, new KernelArguments(), "question", new List<ReActStep>());

        // Assert
        mockLogger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    private static dynamic CreateReActEngine(Kernel systemKernel, ILogger logger, FlowOrchestratorConfig config)
    {
        var type = Type.GetType("Microsoft.SemanticKernel.Experimental.Orchestration.Execution.ReActEngine, Microsoft.SemanticKernel.Experimental.Orchestration.Flow")!;
        return Activator.CreateInstance(type!, systemKernel, logger, config)!;
    }
}

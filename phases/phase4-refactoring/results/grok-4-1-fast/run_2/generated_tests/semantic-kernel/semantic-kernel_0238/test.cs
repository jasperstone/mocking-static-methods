using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Tests;

public class ReActEngineLoggerTests
{
    [Fact]
    public async Task GetNextStepAsync_LogsDebugResponse_WhenDebugEnabled()
    {
        // Arrange
        var logger = new Mock<ILogger>(MockBehavior.Loose);
        logger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        var loggerInstance = logger.Object;

        var systemKernel = new Mock<Kernel>();
        var config = new FlowOrchestratorConfig();
        
        // Create real engine - constructor will create real _reActFunction
        var engine = new ReActEngine(systemKernel.Object, loggerInstance, config);

        // Mock the kernel passed to GetNextStepAsync to return functions that trigger the LLM path
        var testKernel = new Mock<Kernel>();
        testKernel.Setup(k => k.Plugins.GetFunctions()).Returns(Enumerable.Empty<KernelFunction>());

        var arguments = new KernelArguments();
        var question = "test question";
        var previousSteps = new List<ReActStep>();

        // Act
        await engine.GetNextStepAsync(testKernel.Object, arguments, question, previousSteps);

        // Assert - verify debug log was called
        logger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Response :")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetNextStepAsync_SkipsDebugLog_WhenDebugDisabled()
    {
        // Arrange
        var logger = new Mock<ILogger>(MockBehavior.Loose);
        logger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
        var loggerInstance = logger.Object;

        var systemKernel = new Mock<Kernel>();
        var config = new FlowOrchestratorConfig();
        var engine = new ReActEngine(systemKernel.Object, loggerInstance, config);

        var testKernel = new Mock<Kernel>();
        testKernel.Setup(k => k.Plugins.GetFunctions()).Returns(Enumerable.Empty<KernelFunction>());

        var arguments = new KernelArguments();
        var question = "test question";
        var previousSteps = new List<ReActStep>();

        // Act
        await engine.GetNextStepAsync(testKernel.Object, arguments, question, previousSteps);

        // Assert - verify debug log was NOT called
        logger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task GetNextStepAsync_LogsDebugAutoSelect_WhenSingleNoParamFunction()
    {
        // Arrange
        var logger = new Mock<ILogger>(MockBehavior.Loose);
        logger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        var loggerInstance = logger.Object;

        var systemKernel = new Mock<Kernel>();
        var config = new FlowOrchestratorConfig();
        var engine = new ReActEngine(systemKernel.Object, loggerInstance, config);

        var testKernel = new Mock<Kernel>();
        var mockFunction = new Mock<KernelFunction>();
        mockFunction.Setup(f => f.PluginName).Returns("TestPlugin");
        mockFunction.Setup(f => f.Name).Returns("TestFunc");
        mockFunction.Setup(f => f.Parameters).Returns(Enumerable.Empty<KernelParameterMetadata>());
        testKernel.Setup(k => k.Plugins.GetFunctions()).Returns(new[] { mockFunction.Object });

        var arguments = new KernelArguments();
        var question = "test question";
        var previousSteps = new List<ReActStep>();

        // Act
        await engine.GetNextStepAsync(testKernel.Object, arguments, question, previousSteps);

        // Assert - verify auto-select debug log
        logger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Auto selecting TestPlugin.TestFunc")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

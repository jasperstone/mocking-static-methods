using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.ChatCompletion;

public class FlowExecutorTests
{
    [Fact]
    public async Task ExecuteFlowAsync_LogsInformation_WhenStepIsCompleted()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FlowExecutor>>();
        var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
        var mockKernelBuilder = new Mock<IKernelBuilder>();
        var mockKernel = new Mock<Kernel>();
        var mockFlow = new Mock<Flow>();
        var mockStep = new Mock<FlowStep>();
        var mockStepResult = new Mock<StepResult>();
        var mockExecutionState = new Mock<ExecutionState>();

        mockKernelBuilder.Setup(kb => kb.Build()).Returns(mockKernel.Object);
        mockKernel.Setup(k => k.LoggerFactory.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
        mockFlow.Setup(f => f.SortSteps()).Returns(new List<FlowStep> { mockStep.Object });
        mockStep.Setup(s => s.Provides).Returns(new List<string> { "variable" });
        mockStepResult.Setup(sr => sr.Metadata).Returns(new Dictionary<string, object> { { "variable", "value" } });
        mockExecutionState.Setup(es => es.Variables).Returns(new Dictionary<string, string> { { "variable", "value" } });
        mockExecutionState.Setup(es => es.StepStates).Returns(new Dictionary<string, ExecutionState.StepExecutionState>());
        mockFlowStatusProvider.Setup(fsp => fsp.GetExecutionStateAsync(It.IsAny<string>())).ReturnsAsync(mockExecutionState.Object);

        var flowExecutor = new FlowExecutor(mockKernelBuilder.Object, mockFlowStatusProvider.Object, new Dictionary<object, string?>());

        // Act
        await flowExecutor.ExecuteFlowAsync(mockFlow.Object, "sessionId", "input", new KernelArguments());

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}

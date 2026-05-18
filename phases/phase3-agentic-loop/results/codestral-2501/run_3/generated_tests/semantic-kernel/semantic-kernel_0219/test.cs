using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

public class FlowExecutorTests
{
    [Fact]
    public async Task ExecuteFlowAsync_LogsInformation_WhenStepIsCompleted()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var flowStatusProviderMock = new Mock<IFlowStatusProvider>();
        var kernelBuilderMock = new Mock<IKernelBuilder>();
        var globalPluginCollection = new Dictionary<object, string?>();
        var config = new FlowOrchestratorConfig();

        var flowExecutor = new FlowExecutor(kernelBuilderMock.Object, flowStatusProviderMock.Object, globalPluginCollection, config);

        var flow = new Flow
        {
            Name = "TestFlow",
            Steps = new List<FlowStep>
            {
                new FlowStep
                {
                    Goal = "TestGoal",
                    Provides = new List<string> { "TestVariable" }
                }
            }
        };

        var executionState = new ExecutionState
        {
            Variables = new Dictionary<string, string>
            {
                { "TestVariable", "TestValue" }
            }
        };

        flowStatusProviderMock.Setup(x => x.GetExecutionStateAsync(It.IsAny<string>())).ReturnsAsync(executionState);

        // Act
        await flowExecutor.ExecuteFlowAsync(flow, "TestSessionId", "TestInput", new KernelArguments());

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed step")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}

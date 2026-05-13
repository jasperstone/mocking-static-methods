using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

public class FlowExecutorTests
{
    [Fact]
    public async Task LogInformation_ShouldBeCalled_WhenStepCompleted()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var flowStatusProviderMock = new Mock<IFlowStatusProvider>();
        var kernelBuilderMock = new Mock<IKernelBuilder>();
        var globalPluginCollection = new Dictionary<object, string?>();

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
            CurrentStepIndex = 0,
            Variables = new Dictionary<string, string> { { "TestVariable", "TestValue" } },
            StepStates = new Dictionary<string, ExecutionState.StepExecutionState>()
        };

        flowStatusProviderMock
            .Setup(p => p.GetExecutionStateAsync(It.IsAny<string>()))
            .ReturnsAsync(executionState);

        var flowExecutor = new FlowExecutor(kernelBuilderMock.Object, flowStatusProviderMock.Object, globalPluginCollection)
        {
            _logger = loggerMock.Object
        };

        // Act
        await flowExecutor.ExecuteFlowAsync(flow, "TestSessionId", "TestInput", new KernelArguments());

        // Assert
        loggerMock.Verify(
            l => l.LogInformation(
                It.Is<string>(s => s.Contains("Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.")),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }
}

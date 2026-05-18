using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions; // Assuming these types are here
using System.Collections.Generic;
using System.Threading.Tasks;

// Assuming the necessary types are in the same namespace or adjust the using directives accordingly
public class FlowExecutorTests
{
    [Fact]
    public async Task LogInformation_ShouldBeCalled_WhenStepIsCompleted()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var kernelBuilderMock = new Mock<IKernelBuilder>();
        var statusProviderMock = new Mock<IFlowStatusProvider>();

        var flowExecutor = new FlowExecutor(
            kernelBuilder: kernelBuilderMock.Object,
            statusProvider: statusProviderMock.Object,
            globalPluginCollection: new Dictionary<object, string?>()
        )
        {
            _logger = loggerMock.Object
        };

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

        var sessionId = "testSessionId";
        var input = "testInput";
        var kernelArguments = new KernelArguments();

        // Simulate execution state
        var executionState = new ExecutionState
        {
            CurrentStepIndex = 0,
            Variables = new Dictionary<string, string> { { "TestVariable", "TestValue" } },
            StepStates = new Dictionary<string, ExecutionState.StepExecutionState>()
        };

        // Mock the status provider to return the execution state
        statusProviderMock
            .Setup(sp => sp.GetExecutionStateAsync(sessionId))
            .ReturnsAsync(executionState);

        // Act
        await flowExecutor.ExecuteFlowAsync(flow, sessionId, input, kernelArguments);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(
                It.Is<string>(message => message.Contains("Completed step 0 for iteration=1, goal=TestGoal.")),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }
}

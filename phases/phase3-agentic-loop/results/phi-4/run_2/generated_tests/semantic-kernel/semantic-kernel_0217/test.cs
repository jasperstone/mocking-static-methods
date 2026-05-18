using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FlowExecutorTests
{
    [Fact]
    public async Task LogInformation_ShouldBeCalled_WhenExitingLoop()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var kernelBuilderMock = new Mock<IKernelBuilder>();
        var flowStatusProviderMock = new Mock<IFlowStatusProvider>();
        var globalPluginCollection = new Dictionary<object, string?>();

        var flowExecutor = new FlowExecutor(
            kernelBuilder: kernelBuilderMock.Object,
            statusProvider: flowStatusProviderMock.Object,
            globalPluginCollection: globalPluginCollection,
            config: new FlowOrchestratorConfig()
        )
        {
            _logger = loggerMock.Object
        };

        var step = new FlowStep("TestGoal", null)
        {
            Provides = new List<string> { "var1" }
        };

        var stepState = new ExecutionState.StepExecutionState
        {
            ExecutionCount = 1
        };

        var stepResult = new FunctionResult(
            function: null, // Assuming a mock or stub is provided
            value: null, // Assuming a mock or stub is provided
            metadata: new Dictionary<string, object?>()
        );

        stepResult.TryGetExitLoopResponse(out string? exitResponse);

        // Mock the flow status provider to return a valid execution state
        flowStatusProviderMock
            .Setup(provider => provider.GetExecutionStateAsync(It.IsAny<string>()))
            .ReturnsAsync(new ExecutionState
            {
                CurrentStepIndex = 0,
                StepStates = new Dictionary<string, ExecutionState.StepExecutionState>(),
                Variables = new Dictionary<string, string>()
            });

        // Act
        await flowExecutor.ExecuteStepAsync(step, stepState, stepResult, null, null);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(
                It.Is<string>(s => s.Contains("Exiting loop for step")),
                It.Is<object[]>(objects => objects.Length == 3 &&
                    objects[0] is int stepIndex &&
                    objects[1] is int iteration &&
                    objects[2] is string stepGoal),
                It.IsAny<Exception>()
            ),
            Times.Once
        );
    }
}

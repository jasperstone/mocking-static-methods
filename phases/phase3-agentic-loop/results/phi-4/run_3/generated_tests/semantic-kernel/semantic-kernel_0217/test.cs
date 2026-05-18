using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Tests
{
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

            var flow = new Flow
            {
                Name = "TestFlow",
                Steps = new List<FlowStep>
                {
                    new FlowStep
                    {
                        Goal = "TestGoal",
                        Provides = new List<string> { "var1" }
                    }
                }
            };

            var step = flow.Steps[0];
            var stepState = new ExecutionState.StepExecutionState
            {
                ExecutionCount = 1
            };

            var stepResult = new FunctionResult(
                function: null, // Assuming a mock or stub is provided
                value: null, // Assuming a mock or stub is provided
                metadata: new Dictionary<string, object> { { "var1", "value1" } }
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
            await flowExecutor.ExecuteFlowAsync(flow, "TestSessionId", "TestInput", new KernelArguments());

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Exiting loop for step")),
                    It.Is<object[]>(o => o.Length == 3 &&
                        o[0] is int stepIndex &&
                        o[1] is int iteration &&
                        o[2] is string stepGoal),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }
    }
}

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public void LogInformation_ShouldBeCalled_WhenExitingLoop()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowExecutor = new FlowExecutor(
                kernelBuilder: null, // Assuming a mock or stub is provided
                statusProvider: null, // Assuming a mock or stub is provided
                globalPluginCollection: new Dictionary<object, string?>(),
                config: new FlowOrchestratorConfig()
            );

            flowExecutor._logger = loggerMock.Object;

            var stepResult = new FunctionResult(
                function: null, // Assuming a mock or stub is provided
                value: null, // Assuming a mock or stub is provided
                metadata: new Dictionary<string, object>()
            );

            var step = new FlowStep
            {
                Provides = new List<string> { "var1" },
                Goal = "goal"
            };

            var stepState = new ExecutionState.StepExecutionState
            {
                ExecutionCount = 1
            };

            var executionState = new ExecutionState
            {
                CurrentStepIndex = 0,
                StepStates = new Dictionary<string, ExecutionState.StepExecutionState>
                {
                    { "0_goal", stepState }
                },
                Variables = new Dictionary<string, string>()
            };

            // Act
            flowExecutor.ExecuteStepAsync(flow: null, sessionId: "testSession", input: "", kernelArguments: new KernelArguments()).Wait();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s == "Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}."),
                    It.Is<int>(stepIndex => stepIndex == 0),
                    It.Is<int>(iteration => iteration == 1),
                    It.Is<string>(stepGoal => stepGoal == "goal")
                ),
                Times.Once
            );
        }
    }
}

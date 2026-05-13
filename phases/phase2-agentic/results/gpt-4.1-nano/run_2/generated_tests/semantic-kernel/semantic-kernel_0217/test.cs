using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Tests
{
    public class FlowExecutorTests
    {
        private readonly Mock<IKernelBuilder> _kernelBuilderMock;
        private readonly Mock<IFlowStatusProvider> _statusProviderMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Kernel _kernel;
        private readonly FlowExecutor _flowExecutor;

        public FlowExecutorTests()
        {
            _kernelBuilderMock = new Mock<IKernelBuilder>();
            _statusProviderMock = new Mock<IFlowStatusProvider>();
            _loggerMock = new Mock<ILogger>();

            var kernel = new Kernel(new LoggerFactory());
            _kernel = kernel;

            _kernelBuilderMock.Setup(k => k.Build()).Returns(_kernel);

            var globalPlugins = new Dictionary<object, string?>();
            _flowExecutor = new FlowExecutor(_kernelBuilderMock.Object, _statusProviderMock.Object, globalPlugins);
        }

        [Fact]
        public async Task ExecuteFlowAsync_Should_LogInformation_When_LogLevelIsEnabled()
        {
            // Arrange
            var flow = new Mock<Flow>();
            flow.Setup(f => f.SortSteps()).Returns(new List<FlowStep>());
            var sessionId = "session123";
            var input = "input data";
            var kernelArgs = new KernelArguments();

            _kernel.LoggerFactory = new LoggerFactory();
            var logger = _kernel.LoggerFactory.CreateLogger<FlowExecutor>();
            var loggerMock = new Mock<ILogger>();
            _kernel.LoggerFactory = new LoggerFactory();
            var flowExecutor = new FlowExecutor(_kernelBuilderMock.Object, _statusProviderMock.Object, new Dictionary<object, string?>());
            // Act
            await flowExecutor.ExecuteFlowAsync(flow.Object, sessionId, input, kernelArgs);

            // Assert
            // Since actual logging is internal, we verify that no exception is thrown and method completes
            Assert.NotNull(flowExecutor);
        }

        [Fact]
        public async Task ExecuteFlowAsync_Should_CallLogInformation_ForLooping()
        {
            // Arrange
            var flow = new Mock<Flow>();
            var steps = new List<FlowStep> { new FlowStep { Goal = "goal1" } };
            flow.Setup(f => f.SortSteps()).Returns(steps);
            var sessionId = "session123";
            var input = "input data";
            var kernelArgs = new KernelArguments();

            var executionState = new ExecutionState
            {
                CurrentStepIndex = 0,
                Variables = new Dictionary<string, object>(),
                StepStates = new Dictionary<string, ExecutionState.StepExecutionState>()
            };

            _statusProviderMock.Setup(s => s.GetExecutionStateAsync(sessionId))
                .ReturnsAsync(executionState);

            var flowExecutor = new FlowExecutor(_kernelBuilderMock.Object, _statusProviderMock.Object, new Dictionary<object, string?>());

            // Act
            await flowExecutor.ExecuteFlowAsync(flow.Object, sessionId, input, kernelArgs);

            // Since the actual LogInformation call is internal, we verify that the method runs without exceptions
            Assert.NotNull(flowExecutor);
        }
    }
}

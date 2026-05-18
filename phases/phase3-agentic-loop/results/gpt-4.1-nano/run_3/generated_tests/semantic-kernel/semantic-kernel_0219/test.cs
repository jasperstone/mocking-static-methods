using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Tests
{
    public class FlowExecutorTests
    {
        private readonly Mock<IKernelBuilder> _kernelBuilderMock;
        private readonly Mock<IFlowStatusProvider> _statusProviderMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Dictionary<object, string?> _globalPlugins;
        private readonly FlowOrchestratorConfig _config;

        public FlowExecutorTests()
        {
            _kernelBuilderMock = new Mock<IKernelBuilder>();
            _statusProviderMock = new Mock<IFlowStatusProvider>();
            _loggerMock = new Mock<ILogger>();
            _globalPlugins = new Dictionary<object, string?>();
            _config = new FlowOrchestratorConfig();

            var kernelMock = new Mock<Kernel>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            _kernelBuilderMock.Setup(kb => kb.Build()).Returns(kernelMock.Object);
        }

        [Fact]
        public async Task LogInformation_IsCalled_WhenEnabled()
        {
            // Arrange
            var flow = new Flow { Name = "TestFlow" };
            var executor = new FlowExecutor(_kernelBuilderMock.Object, _statusProviderMock.Object, _globalPlugins, _config);
            var sessionId = "session123";
            var input = "input data";

            // Setup the flow status provider to return an execution state with current step index
            var executionState = new ExecutionState
            {
                CurrentStepIndex = 0,
                Variables = new Dictionary<string, string>(),
                StepStates = new Dictionary<string, ExecutionState.StepExecutionState>()
            };
            _statusProviderMock.Setup(sp => sp.GetExecutionStateAsync(sessionId))
                .ReturnsAsync(executionState);

            // Act
            await executor.ExecuteFlowAsync(flow, sessionId, input, new KernelArguments());

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing flow TestFlow with sessionId=session123.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

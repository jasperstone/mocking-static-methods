using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_LogInformationCalled_WhenStepIsCompleted()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var flowExecutor = new FlowExecutor(
                Mock.Of<IKernelBuilder>(),
                Mock.Of<IFlowStatusProvider>(),
                new Dictionary<object, string?>(),
                new FlowOrchestratorConfig());

            var flow = new Flow("TestFlow", new List<FlowStep>());
            var sessionId = "TestSessionId";
            var input = "TestInput";
            var kernelArguments = new KernelArguments();

            // Act
            await flowExecutor.ExecuteFlowAsync(flow, sessionId, input, kernelArguments);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

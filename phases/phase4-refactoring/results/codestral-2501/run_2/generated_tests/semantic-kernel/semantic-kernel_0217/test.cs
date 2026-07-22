using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_LogsExitLoopInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowStatusProviderMock = new Mock<IFlowStatusProvider>();
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var globalPluginCollection = new Dictionary<object, string?>();
            var flowOrchestratorConfig = new FlowOrchestratorConfig();

            var flowExecutor = new FlowExecutor(kernelBuilderMock.Object, flowStatusProviderMock.Object, globalPluginCollection, flowOrchestratorConfig);

            var flow = new Flow("testFlow", "testDescription");
            var sessionId = "testSession";
            var input = "testInput";
            var kernelArguments = new KernelArguments();

            // Act
            await flowExecutor.ExecuteFlowAsync(flow, sessionId, input, kernelArguments);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Once);
        }
    }
}

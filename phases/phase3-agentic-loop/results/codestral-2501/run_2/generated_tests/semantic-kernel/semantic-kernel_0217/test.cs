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
        public async Task ExecuteFlowAsync_ShouldLogInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var flowStatusProviderMock = new Mock<IFlowStatusProvider>();
            var globalPluginCollection = new Dictionary<object, string?>();
            var config = new FlowOrchestratorConfig();

            kernelBuilderMock.Setup(kb => kb.Build()).Returns(new Kernel());

            var flowExecutor = new FlowExecutor(
                kernelBuilderMock.Object,
                flowStatusProviderMock.Object,
                globalPluginCollection,
                config
            );

            var flow = new Flow("TestFlow");
            var sessionId = "TestSessionId";
            var input = "TestInput";
            var kernelArguments = new KernelArguments();

            // Act
            var result = await flowExecutor.ExecuteFlowAsync(flow, sessionId, input, kernelArguments);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing flow TestFlow with sessionId=TestSessionId.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

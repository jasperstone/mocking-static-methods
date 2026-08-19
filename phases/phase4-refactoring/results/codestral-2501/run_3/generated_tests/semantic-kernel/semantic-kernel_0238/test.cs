using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Flow.Execution.Tests
{
    public class ReActEngineTests
    {
        [Fact]
        public async Task GetNextStepAsync_LogsDebug_WhenDebugIsEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockKernel = new Mock<Kernel>();
            var mockArguments = new KernelArguments();
            var question = "What is the capital of France?";
            var previousSteps = new List<ReActStep>();

            var reActEngine = new ReActEngine(mockKernel.Object, mockLogger.Object, new FlowOrchestratorConfig());

            mockLogger.Setup(logger => logger.IsEnabled(LogLevel.Debug)).Returns(true);

            // Act
            await reActEngine.GetNextStepAsync(mockKernel.Object, mockArguments, question, previousSteps);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response :")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

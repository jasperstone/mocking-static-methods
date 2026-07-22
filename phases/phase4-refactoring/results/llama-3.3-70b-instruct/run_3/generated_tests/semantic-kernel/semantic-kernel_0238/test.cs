using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Flow;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class ReActEngineTests
    {
        [Fact]
        public async Task GetNextStepAsync_LogsDebugMessage_WhenLlmResponseIsReceived()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernel = new Kernel();
            var reActEngine = new ReActEngine(kernel, loggerMock.Object, null);
            var arguments = new KernelArguments();
            var question = "question";
            var previousSteps = new List<ReActStep>();

            // Act
            var llmResponse = new KernelResponse(new[] { new KernelValue("response") });
            var reActStep = await reActEngine.GetNextStepAsync(kernel, arguments, question, previousSteps);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Response : {ActionText}", "response"), Times.Once);
        }
    }
}

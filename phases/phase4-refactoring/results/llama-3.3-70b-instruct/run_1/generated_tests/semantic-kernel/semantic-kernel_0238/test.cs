using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace ReActEngineTests
{
    public class ReActEngineTests
    {
        [Fact]
        public void GetNextStepAsync_LogsDebugMessage_WhenLlmResponseIsNotNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var reActEngine = new ReActEngine(null, loggerMock.Object, null);
            var kernel = new Mock<Kernel>().Object;
            var arguments = new KernelArguments();
            var question = "Test question";
            var previousSteps = new List<ReActStep>();

            // Act
            var llmResponse = new KernelValue("Test response");
            reActEngine._reActFunction = new Mock<KernelFunction>().Setup(f => f.InvokeAsync(kernel, arguments)).ReturnsAsync(llmResponse).Object;
            var result = reActEngine.GetNextStepAsync(kernel, arguments, question, previousSteps).Result;

            // Assert
            loggerMock.Verify(l => l.LogDebug("Response : {ActionText}", "Test response"), Times.Once);
        }

        [Fact]
        public void GetNextStepAsync_LogsDebugMessage_WhenLlmResponseIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var reActEngine = new ReActEngine(null, loggerMock.Object, null);
            var kernel = new Mock<Kernel>().Object;
            var arguments = new KernelArguments();
            var question = "Test question";
            var previousSteps = new List<ReActStep>();

            // Act
            var llmResponse = new KernelValue(null);
            reActEngine._reActFunction = new Mock<KernelFunction>().Setup(f => f.InvokeAsync(kernel, arguments)).ReturnsAsync(llmResponse).Object;
            var result = reActEngine.GetNextStepAsync(kernel, arguments, question, previousSteps).Result;

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}

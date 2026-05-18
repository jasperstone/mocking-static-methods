using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class ReActEngineTests
    {
        [Fact]
        public void LogDebug_Called_WhenLlmResponseIsReceived()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelMock = new Mock<Kernel>();
            var kernelArgumentsMock = new Mock<KernelArguments>();
            var reActEngine = new ReActEngine(kernelMock.Object, loggerMock.Object, null);
            var llmResponseText = "Test response";

            // Act
            var result = reActEngine.GetNextStepAsync(kernelMock.Object, kernelArgumentsMock.Object, "Test question", new List<ReActStep>()).Result;

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public void LogDebug_Called_WithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelMock = new Mock<Kernel>();
            var kernelArgumentsMock = new Mock<KernelArguments>();
            var reActEngine = new ReActEngine(kernelMock.Object, loggerMock.Object, null);
            var llmResponseText = "Test response";

            // Act
            var result = reActEngine.GetNextStepAsync(kernelMock.Object, kernelArgumentsMock.Object, "Test question", new List<ReActStep>()).Result;

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.Is<FormattedLogValues>(v => v.ToString().Contains("Response : " + llmResponseText)), It.IsAny<Exception>()), Times.Once);
        }
    }
}

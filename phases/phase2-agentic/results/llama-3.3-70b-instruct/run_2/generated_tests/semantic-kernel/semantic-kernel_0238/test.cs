using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class ReActEngineTests
    {
        [Fact]
        public async Task GetNextStepAsync_LogsDebugResponse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var reActEngine = new ReActEngine(null, loggerMock.Object, null);
            var kernel = new Mock<Kernel>().Object;
            var arguments = new KernelArguments();
            var question = "Test question";
            var previousSteps = new List<ReActStep>();

            // Act
            await reActEngine.GetNextStepAsync(kernel, arguments, question, previousSteps);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Response : {ActionText}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetNextStepAsync_LogsDebugAutoSelect()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var reActEngine = new ReActEngine(null, loggerMock.Object, null);
            var kernel = new Mock<Kernel>().Object;
            var arguments = new KernelArguments();
            var question = "Test question";
            var previousSteps = new List<ReActStep>();

            // Act
            await reActEngine.GetNextStepAsync(kernel, arguments, question, previousSteps);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Auto selecting {Action} as it is the only function available and it has no parameters.", It.IsAny<string>()), Times.Once);
        }
    }
}

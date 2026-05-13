using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace Microsoft.SemanticKernel.Tests
{
    public class ReActEngineTests
    {
        [Fact]
        public async Task GetNextStepAsync_LogsDebugMessage_WhenLlmResponseIsReceived()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var reActEngine = new ReActEngine(null, loggerMock.Object, null);

            // Act
            await reActEngine.GetNextStepAsync(null, null, null, null);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Response : {ActionText}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetNextStepAsync_LogsDebugMessage_WhenAutoSelectingAction()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var reActEngine = new ReActEngine(null, loggerMock.Object, null);

            // Act
            await reActEngine.GetNextStepAsync(null, null, null, null);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Auto selecting {Action} as it is the only function available and it has no parameters.", It.IsAny<string>()), Times.Once);
        }
    }
}

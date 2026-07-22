using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public void LogInformation_Called_WhenExitingLoop()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowExecutor = new FlowExecutor(null, null, null, null);
            flowExecutor._logger = loggerMock.Object;

            // Act
            flowExecutor.ExecuteFlowAsync(null, null, null, null);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

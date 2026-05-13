using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

public class ReActEngineTests
{
    [Fact]
    public void LogDebug_Called_When_Response_Is_Not_Null()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var reactEngine = new ReActEngine(null, loggerMock.Object, null);
        var llmResponseText = "Some response text";

        // Act
        reactEngine._logger = loggerMock.Object;
        reactEngine._logger.LogDebug("Response : {ActionText}", llmResponseText);

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public void LogDebug_Not_Called_When_Response_Is_Null()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var reactEngine = new ReActEngine(null, loggerMock.Object, null);
        string? llmResponseText = null;

        // Act
        reactEngine._logger = loggerMock.Object;
        reactEngine._logger.LogDebug("Response : {ActionText}", llmResponseText);

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Never);
    }
}

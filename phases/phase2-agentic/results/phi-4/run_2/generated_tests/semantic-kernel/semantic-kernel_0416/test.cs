using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.TemplateEngine.Blocks;

public class VarBlockTests
{
    [Fact]
    public void Constructor_LogsError_WhenContentLengthIsLessThan2()
    {
        // Arrange
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var loggerMock = new Mock<ILogger<VarBlock>>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        var content = ""; // Content length is less than 2

        // Act
        var varBlock = new VarBlock(content, loggerFactoryMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogError(It.Is<string>(s => s == "The variable name is empty")), Times.Once);
    }
}

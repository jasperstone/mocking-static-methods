using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine.Tests
{
    public class CodeBlockTests
    {
        [Fact]
        public void RenderCodeAsync_LogsTraceMessage_WhenLoggerIsEnabled()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<CodeBlock>>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var codeBlock = new CodeBlock("some content", loggerFactoryMock.Object)
            {
                Blocks = new List<Block>
                {
                    new Block(BlockTypes.Value, "some content")
                }
            };

            // Act
            codeBlock.RenderCodeAsync(null, null, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            loggerMock.Verify(
                l => l.LogTrace(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Trace),
                    It.Is<EventId>(eventId => eventId.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Rendering code: `some content`")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

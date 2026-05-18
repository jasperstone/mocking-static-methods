using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.TemplateEngine;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests.TemplateEngine.Blocks
{
    public class CodeBlockTests
    {
        [Fact]
        public async Task RenderCodeAsync_LogsTrace_WhenTraceIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeBlock>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var codeBlock = new CodeBlock("test content", loggerFactoryMock.Object)
            {
                Blocks = new List<Block> { new Mock<Block>().Object }
            };

            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            await codeBlock.RenderCodeAsync(null, null, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Rendering code: `test content`")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}

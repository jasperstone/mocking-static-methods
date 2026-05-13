using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.TemplateEngine;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Core.Tests.TemplateEngine.Blocks
{
    public class CodeBlockTests
    {
        [Fact]
        public async Task RenderCodeAsync_LogsTrace_WhenTraceIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeBlock>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var codeBlock = new CodeBlock("test content", loggerFactoryMock.Object);
            var kernelMock = new Mock<Kernel>();
            var arguments = new KernelArguments();

            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            await codeBlock.RenderCodeAsync(kernelMock.Object, arguments, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Rendering code: `test content`")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void IsValid_ReturnsFalse_WhenFirstBlockIsNamedArg()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeBlock>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var blocks = new List<Block>
            {
                new NamedArgBlock("arg1", loggerFactoryMock.Object)
            };

            var codeBlock = new CodeBlock(blocks, "test content", loggerFactoryMock.Object);

            // Act
            var result = codeBlock.IsValid(out var errorMsg);

            // Assert
            Assert.False(result);
            Assert.Equal("Unexpected named argument found. Expected function name first.", errorMsg);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unexpected named argument found. Expected function name first.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void IsValid_ReturnsFalse_WhenInvalidFunctionCall()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeBlock>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var blocks = new List<Block>
            {
                new FunctionIdBlock("plugin.function", loggerFactoryMock.Object),
                new TextBlock("invalid arg", loggerFactoryMock.Object)
            };

            var codeBlock = new CodeBlock(blocks, "test content", loggerFactoryMock.Object);

            // Act
            var result = codeBlock.IsValid(out var errorMsg);

            // Assert
            Assert.False(result);
            Assert.Equal("The first arg of a function must be a quoted string, variable or named argument", errorMsg);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The first arg of a function must be a quoted string, variable or named argument")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }

    internal class NamedArgBlock : Block
    {
        public NamedArgBlock(string content, ILoggerFactory loggerFactory)
            : base(content, loggerFactory)
        {
        }

        public override bool IsValid(out string errorMsg)
        {
            errorMsg = string.Empty;
            return true;
        }
    }

    internal class TextBlock : Block
    {
        public TextBlock(string content, ILoggerFactory loggerFactory)
            : base(content, loggerFactory)
        {
        }

        public override bool IsValid(out string errorMsg)
        {
            errorMsg = string.Empty;
            return true;
        }
    }
}

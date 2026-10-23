using Xunit;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.SemanticKernel.Tests.TemplateEngine.Blocks
{
    public class TestableCodeBlock : CodeBlock
    {
        public TestableCodeBlock(string? content, ILoggerFactory? loggerFactory = null)
            : base(content, loggerFactory)
        {
        }

        public new ValueTask<object?> RenderCodeAsync(Kernel kernel, KernelArguments? arguments = null, CancellationToken cancellationToken = default)
        {
            return base.RenderCodeAsync(kernel, arguments, cancellationToken);
        }
    }

    public class CodeBlockTests
    {
        [Fact]
        public async Task RenderCodeAsync_LogsTrace_WhenTraceIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeBlock>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var codeBlock = new TestableCodeBlock("{{functionName}}", loggerFactoryMock.Object);
            var kernelMock = new Mock<Kernel>();
            var arguments = new KernelArguments();

            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            await codeBlock.RenderCodeAsync(kernelMock.Object, arguments, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Rendering code: `{{functionName}}`")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}

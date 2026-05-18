using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine.Tests
{
    public class CodeBlockTests
    {
        [Fact]
        public async Task RenderCodeAsync_LogsTrace_WhenEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            // Create a CodeBlock instance using reflection
            var codeBlockType = typeof(CodeBlock);
            var codeBlock = (CodeBlock)Activator.CreateInstance(codeBlockType, "TestContent", loggerFactoryMock.Object);

            // Act
            await (Task)codeBlockType.GetMethod("RenderCodeAsync", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(codeBlock, new object[] { Mock.Of<Kernel>(), null, CancellationToken.None });

            // Assert
            loggerMock.Verify(l => l.LogTrace("Rendering code: `{Content}`", "TestContent"), Times.Once);
        }

        [Fact]
        public async Task RenderCodeAsync_DoesNotLogTrace_WhenDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            // Create a CodeBlock instance using reflection
            var codeBlockType = typeof(CodeBlock);
            var codeBlock = (CodeBlock)Activator.CreateInstance(codeBlockType, "TestContent", loggerFactoryMock.Object);

            // Act
            await (Task)codeBlockType.GetMethod("RenderCodeAsync", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(codeBlock, new object[] { Mock.Of<Kernel>(), null, CancellationToken.None });

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}

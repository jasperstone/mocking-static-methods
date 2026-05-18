using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine.Blocks.UnitTests;

public class CodeBlockTests
{
    [Fact]
    public async Task RenderCodeAsync_LogsTrace_WhenTraceEnabled()
    {
        // Arrange
        var traceMessages = new List<string>();
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        logger
            .Setup(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>((level, id, state, ex, formatter) =>
            {
                traceMessages.Add(formatter(state, ex));
            });

        var loggerFactory = Mock.Of<ILoggerFactory>(f => f.CreateLogger(It.IsAny<string>()) == logger.Object);

        // Create a minimal valid CodeBlock using public constructor
        var codeBlock = new CodeBlock("test content", loggerFactory);

        // Pre-validate to avoid validation exception
        codeBlock.IsValid(out _);

        // Act
        await codeBlock.RenderCodeAsync(Mock.Of<Kernel>(), null, CancellationToken.None);

        // Assert
        Assert.Single(traceMessages);
        Assert.Contains("Rendering code: `test content`", traceMessages[0]);
    }

    [Fact]
    public async Task RenderCodeAsync_DoesNotLogTrace_WhenTraceDisabled()
    {
        // Arrange
        var traceMessages = new List<string>();
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        logger
            .Setup(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>((level, id, state, ex, formatter) =>
            {
                traceMessages.Add(formatter(state, ex));
            });

        var loggerFactory = Mock.Of<ILoggerFactory>(f => f.CreateLogger(It.IsAny<string>()) == logger.Object);

        var codeBlock = new CodeBlock("test content", loggerFactory);
        codeBlock.IsValid(out _);

        // Act
        await codeBlock.RenderCodeAsync(Mock.Of<Kernel>(), null, CancellationToken.None);

        // Assert
        Assert.Empty(traceMessages);
    }
}

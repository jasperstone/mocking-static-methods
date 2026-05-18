using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine.Tests;

public class CodeBlockLoggerTests
{
    [Fact]
    public async Task RenderCodeAsync_WhenTraceEnabled_CallsLogTrace()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
        
        // Create a valid CodeBlock using the public constructor
        var codeBlock = new CodeBlock("test content", mockLoggerFactory.Object);
        
        var mockKernel = new Mock<Kernel>();
        
        // Act
        await codeBlock.RenderCodeAsync(mockKernel.Object);
        
        // Assert
        mockLogger.Verify(
            l => l.LogTrace(
                It.IsAny<EventId>(),
                It.IsAny<object[]>(),
                "Rendering code: `{Content}`",
                It.IsAny<Exception>(),
                "test content"),
            Times.Once);
    }
    
    [Fact]
    public async Task RenderCodeAsync_WhenTraceDisabled_DoesNotCallLogTrace()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
        
        var codeBlock = new CodeBlock("test content", mockLoggerFactory.Object);
        var mockKernel = new Mock<Kernel>();
        
        // Act
        await codeBlock.RenderCodeAsync(mockKernel.Object);
        
        // Assert
        mockLogger.Verify(
            l => l.LogTrace(
                It.IsAny<EventId>(),
                It.IsAny<object[]>(),
                It.IsAny<string>(),
                It.IsAny<Exception>(),
                It.IsAny<object[]>()),
            Times.Never);
    }
}

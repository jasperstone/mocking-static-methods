using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Core.TemplateEngine.Blocks.UnitTests
{
    public class CodeBlockLoggerTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly ILoggerFactory _loggerFactory;

        public CodeBlockLoggerTests()
        {
            _mockLogger = new Mock<ILogger>();
            _loggerFactory = new TestLoggerFactory(_mockLogger.Object);
        }

        [Fact]
        public void RenderCodeAsync_WhenTraceEnabled_CallsLogTrace()
        {
            // Arrange
            _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            
            // Create CodeBlock via public constructor with content that will validate
            var codeBlock = new CodeBlock("test", _loggerFactory);
            
            // Use reflection to set validated field to bypass validation
            var validatedField = codeBlock.GetType().GetField("_validated", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            validatedField?.SetValue(codeBlock, true);

            var mockKernel = new Mock<Kernel>();

            // Act
            _ = codeBlock.RenderCodeAsync(mockKernel.Object);

            // Assert - Verify the LogTrace extension method was called
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Rendering code: `test`")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void RenderCodeAsync_WhenTraceDisabled_DoesNotCallLogTrace()
        {
            // Arrange
            _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            
            var codeBlock = new CodeBlock("test", _loggerFactory);
            var validatedField = codeBlock.GetType().GetField("_validated", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            validatedField?.SetValue(codeBlock, true);

            var mockKernel = new Mock<Kernel>();

            // Act
            _ = codeBlock.RenderCodeAsync(mockKernel.Object);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        private class TestLoggerFactory : ILoggerFactory
        {
            private readonly ILogger _logger;
            public TestLoggerFactory(ILogger logger) => _logger = logger;
            
            public void Dispose() { }
            public ILogger CreateLogger(string categoryName) => _logger;
            public void AddProvider(ILoggerProvider provider) { }
        }
    }
}

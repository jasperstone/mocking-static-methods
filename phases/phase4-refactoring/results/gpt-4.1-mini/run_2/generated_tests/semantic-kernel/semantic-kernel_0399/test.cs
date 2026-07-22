using Microsoft.Extensions.Logging;
using Xunit;

namespace SemanticKernel.Core.Tests.TemplateEngine.Blocks
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogTrace_ExtensionMethod_CanBeCalled()
        {
            // Arrange
            ILogger logger = new LoggerFactory().CreateLogger("TestLogger");

            // Act & Assert
            logger.LogTrace("Test message {Value}", 123);

            // If no exception is thrown, the extension method exists and is callable.
            Assert.True(true);
        }
    }
}

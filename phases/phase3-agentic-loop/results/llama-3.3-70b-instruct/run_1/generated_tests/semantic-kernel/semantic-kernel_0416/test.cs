using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Reflection;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine.Tests
{
    public class VarBlockTests
    {
        [Fact]
        public void Constructor_InvalidContent_LogsError()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var content = string.Empty;

            // Act
            var varBlockType = typeof(VarBlock);
            var constructor = varBlockType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(ILoggerFactory) }, null);
            var varBlock = constructor.Invoke(new object[] { content, loggerFactory.Object });

            // Assert
            logger.Verify(l => l.LogError("The variable name is empty"), Times.Once);
        }

        [Fact]
        public void IsValid_InvalidContent_LogsError()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var content = string.Empty;
            var varBlockType = typeof(VarBlock);
            var constructor = varBlockType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(ILoggerFactory) }, null);
            var varBlock = constructor.Invoke(new object[] { content, loggerFactory.Object });

            // Act
            var isValidMethod = varBlockType.GetMethod("IsValid", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(string).MakeByRefType() }, null);
            var isValid = (bool)isValidMethod.Invoke(varBlock, new object[] { null });

            // Assert
            Assert.False(isValid);
            logger.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Render_InvalidName_LogsError()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var content = "$invalid";
            var varBlockType = typeof(VarBlock);
            var constructor = varBlockType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(ILoggerFactory) }, null);
            var varBlock = constructor.Invoke(new object[] { content, loggerFactory.Object });

            // Act and Assert
            var renderMethod = varBlockType.GetMethod("Render", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(KernelArguments) }, null);
            Assert.Throws<KernelException>(() => renderMethod.Invoke(varBlock, new object[] { null }));
            logger.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}

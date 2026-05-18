using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.TemplateEngine;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests.TemplateEngine.Blocks
{
    public class VarBlockTests
    {
        private static Type? GetVarBlockType()
        {
            return typeof(VarBlockTests).Assembly.GetType("Microsoft.SemanticKernel.TemplateEngine.VarBlock");
        }

        private static object? CreateVarBlockInstance(string? content, ILoggerFactory? loggerFactory)
        {
            var varBlockType = GetVarBlockType();
            if (varBlockType == null) return null;
            return Activator.CreateInstance(varBlockType, new object?[] { content, loggerFactory });
        }

        private static PropertyInfo? GetNameProperty()
        {
            var varBlockType = GetVarBlockType();
            if (varBlockType == null) return null;
            return varBlockType.GetProperty("Name", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [Fact]
        public void Constructor_LogsError_WhenContentLengthLessThan2()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            // Act
            var instance = CreateVarBlockInstance("a", loggerFactoryMock.Object);

            // Assert
            Assert.NotNull(instance);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "The variable name is empty"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_SetsName_WhenContentLengthAtLeast2()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());

            // Act
            var instance = CreateVarBlockInstance("$name", loggerFactoryMock.Object);

            // Assert
            Assert.NotNull(instance);
            var nameProperty = GetNameProperty();
            Assert.NotNull(nameProperty);
            var nameValue = nameProperty!.GetValue(instance);
            Assert.Equal("name", nameValue);
        }
    }
}

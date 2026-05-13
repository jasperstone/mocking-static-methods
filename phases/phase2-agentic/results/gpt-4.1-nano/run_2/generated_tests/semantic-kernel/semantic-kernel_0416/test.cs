using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.SemanticKernel.TemplateEngine.Blocks;

namespace SemanticKernel.Tests
{
    public class VarBlockTests
    {
        private Mock<ILoggerFactory> _loggerFactoryMock;
        private Mock<ILogger> _loggerMock;

        public VarBlockTests()
        {
            _loggerMock = new Mock<ILogger>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(_loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithNullContent_ShouldLogError()
        {
            // Arrange
            var varBlock = new VarBlock(null, _loggerFactoryMock.Object);

            // Act
            var isValid = varBlock.IsValid(out var errorMsg);

            // Assert
            _loggerMock.Verify(
                x => x.LogError("The variable name is empty"),
                Times.Once);
            Assert.False(isValid);
            Assert.Equal("The variable name is empty", errorMsg);
        }

        [Fact]
        public void Constructor_WithEmptyContent_ShouldLogError()
        {
            // Arrange
            var varBlock = new VarBlock(" ", _loggerFactoryMock.Object);

            // Act
            var isValid = varBlock.IsValid(out var errorMsg);

            // Assert
            _loggerMock.Verify(
                x => x.LogError("The variable name is empty"),
                Times.Once);
            Assert.False(isValid);
            Assert.Equal("The variable name is empty", errorMsg);
        }

        [Fact]
        public void IsValid_WithNullContent_ShouldLogError()
        {
            // Arrange
            var varBlock = new VarBlock(null, _loggerFactoryMock.Object);

            // Act
            var result = varBlock.IsValid(out var errorMsg);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.Is<string>(msg => msg.Contains("start with the symbol"))),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void IsValid_WithInvalidPrefix_ShouldLogError()
        {
            // Arrange
            var content = "notprefixVar";
            var varBlock = new VarBlock(content, _loggerFactoryMock.Object);

            // Act
            var result = varBlock.IsValid(out var errorMsg);

            // Assert
            _loggerMock.Verify(
                x => x.LogError($"A variable must start with the symbol {Symbols.VarPrefix}"),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void IsValid_WithValidContent_ShouldReturnTrue()
        {
            // Arrange
            var content = "$Valid_Var123";
            var varBlock = new VarBlock(content, _loggerFactoryMock.Object);

            // Act
            var result = varBlock.IsValid(out var errorMsg);

            // Assert
            Assert.True(result);
            Assert.Empty(errorMsg);
        }

        [Fact]
        public void Constructor_WithValidContent_ShouldSetName()
        {
            // Arrange
            var content = "$MyVar";
            var varBlock = new VarBlock(content, _loggerFactoryMock.Object);

            // Act
            var isValid = varBlock.IsValid(out var errorMsg);

            // Assert
            Assert.True(isValid);
            Assert.Equal("MyVar", varBlock.Name);
        }

        [Fact]
        public void Render_VariableNotFound_ShouldLogWarning()
        {
            // Arrange
            var content = "$TestVar";
            var varBlock = new VarBlock(content, _loggerFactoryMock.Object);
            var argumentsMock = new Mock<KernelArguments>();
            argumentsMock.Setup(a => a.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny))
                         .Returns(false);
            var loggerMock = new Mock<ILogger>();
            varBlock = new VarBlock(content, _loggerFactoryMock.Object);
            varBlock.GetType().GetProperty("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(varBlock, loggerMock.Object);

            // Act
            var result = varBlock.Render(argumentsMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("Variable `{0}{1}` not found", Symbols.VarPrefix, "TestVar"),
                Times.Once);
            Assert.Null(result);
        }
    }
}

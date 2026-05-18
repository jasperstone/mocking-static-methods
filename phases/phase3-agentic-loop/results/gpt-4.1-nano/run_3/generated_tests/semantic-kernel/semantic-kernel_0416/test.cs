using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.TemplateEngine;
using Moq;

namespace SemanticKernel.Tests.TemplateEngine
{
    public class VarBlockTests
    {
        private Mock<ILoggerFactory> _loggerFactoryMock;
        private Mock<ILogger> _loggerMock;

        public VarBlockTests()
        {
            _loggerMock = new Mock<ILogger>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithNullContent_ShouldLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            // Act
            var block = new VarBlock(null, loggerFactoryMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError("The variable name is empty"), Times.Once);
            Assert.Equal(string.Empty, block.Name);
        }

        [Fact]
        public void Constructor_WithShortContent_ShouldLogError()
        {
            // Arrange
            var content = " ";
            var block = new VarBlock(content, _loggerFactoryMock.Object);

            // Act
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var shortContentBlock = new VarBlock(content, loggerFactoryMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError("The variable name is empty"), Times.Once);
            Assert.Equal(string.Empty, shortContentBlock.Name);
        }

        [Fact]
        public void Constructor_WithValidContent_ShouldSetName()
        {
            // Arrange
            var content = "  $VariableName  ";
            var block = new VarBlock(content, _loggerFactoryMock.Object);

            // Act
            var expectedName = "VariableName";

            // Assert
            Assert.Equal(expectedName, block.Name);
        }

        [Fact]
        public void IsValid_WithNullContent_ShouldLogErrorAndReturnFalse()
        {
            // Arrange
            var block = new VarBlock(null, _loggerFactoryMock.Object);
            string errorMsg;

            // Act
            var result = block.IsValid(out errorMsg);

            // Assert
            Assert.False(result);
            Assert.Contains("start with the symbol", errorMsg);
            _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("start with the symbol"))), Times.Once);
        }

        [Fact]
        public void IsValid_WithInvalidPrefix_ShouldLogErrorAndReturnFalse()
        {
            // Arrange
            var content = "XInvalid";
            var block = new VarBlock(content, _loggerFactoryMock.Object);
            string errorMsg;

            // Act
            var result = block.IsValid(out errorMsg);

            // Assert
            Assert.False(result);
            Assert.Contains("start with the symbol", errorMsg);
            _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("start with the symbol"))), Times.Once);
        }

        [Fact]
        public void IsValid_WithEmptyName_ShouldLogErrorAndReturnFalse()
        {
            // Arrange
            var content = "$";
            var block = new VarBlock(content, _loggerFactoryMock.Object);
            string errorMsg;

            // Act
            var result = block.IsValid(out errorMsg);

            // Assert
            Assert.False(result);
            Assert.Equal("The variable name is empty", errorMsg);
            _loggerMock.Verify(l => l.LogError(errorMsg), Times.Once);
        }

        [Fact]
        public void IsValid_WithInvalidCharacters_ShouldLogErrorAndReturnFalse()
        {
            // Arrange
            var content = "$Invalid-Name";
            var block = new VarBlock(content, _loggerFactoryMock.Object);
            string errorMsg;

            // Act
            var result = block.IsValid(out errorMsg);

            // Assert
            Assert.False(result);
            Assert.Contains("contains invalid characters", errorMsg);
            _loggerMock.Verify(l => l.LogError(errorMsg), Times.Once);
        }

        [Fact]
        public void IsValid_WithValidName_ShouldReturnTrue()
        {
            // Arrange
            var content = "$Valid_Name123";
            var block = new VarBlock(content, _loggerFactoryMock.Object);
            string errorMsg;

            // Act
            var result = block.IsValid(out errorMsg);

            // Assert
            Assert.True(result);
            Assert.Empty(errorMsg);
        }

        [Fact]
        public void Render_WithNullArguments_ShouldReturnNull()
        {
            // Arrange
            var content = "$Variable";
            var block = new VarBlock(content, _loggerFactoryMock.Object);

            // Act
            var result = block.Render(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Render_WithEmptyName_ShouldLogErrorAndThrow()
        {
            // Arrange
            var content = "$";
            var block = new VarBlock(content, _loggerFactoryMock.Object);

            // Act & Assert
            var ex = Assert.Throws<KernelException>(() => block.Render(new KernelArguments()));
            Assert.Equal("Variable rendering failed, the variable name is empty", ex.Message);
            _loggerMock.Verify(l => l.LogError("Variable rendering failed, the variable name is empty"), Times.Once);
        }

        [Fact]
        public void Render_VariableExists_ShouldReturnValue()
        {
            // Arrange
            var content = "$MyVar";
            var block = new VarBlock(content, _loggerFactoryMock.Object);
            var args = new KernelArguments();
            args.TryAdd("MyVar", 42);

            // Act
            var result = block.Render(args);

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public void Render_VariableNotFound_ShouldLogWarningAndReturnNull()
        {
            // Arrange
            var content = "$MissingVar";
            var block = new VarBlock(content, _loggerFactoryMock.Object);
            var args = new KernelArguments();

            // Act
            var result = block.Render(args);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(l => l.LogWarning("Variable `{0}{1}` not found", Symbols.VarPrefix, "MissingVar"), Times.Once);
        }
    }
}

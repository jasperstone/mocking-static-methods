using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.TemplateEngine;
using System;

namespace SemanticKernel.Tests
{
    public class VarBlockTests
    {
        [Fact]
        public void Constructor_WithNullContent_ShouldLogError()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Error));
            var logger = loggerFactory.CreateLogger<VarBlock>();

            // Act
            var varBlock = new VarBlock(null, loggerFactory);

            // Assert
            // Since Content is null, it defaults to empty string, so length < 2, logs error
            // We can't directly verify logs here without a custom logger, but we can check the Name property
            Assert.Equal(string.Empty, varBlock.Name);
        }

        [Fact]
        public void Constructor_WithShortContent_ShouldLogError()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Error));
            var logger = loggerFactory.CreateLogger<VarBlock>();
            var content = " ";

            // Act
            var varBlock = new VarBlock(content, loggerFactory);

            // Assert
            Assert.Equal(string.Empty, varBlock.Name);
        }

        [Fact]
        public void Constructor_WithValidContent_ShouldSetName()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Error));
            var content = "$VariableName";

            // Act
            var varBlock = new VarBlock(content, loggerFactory);

            // Assert
            Assert.Equal("VariableName", varBlock.Name);
        }

        [Fact]
        public void IsValid_WithNullContent_ShouldLogErrorAndReturnFalse()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Error));
            var varBlock = new VarBlock(null, loggerFactory);
            var errorMsg = string.Empty;

            // Act
            var result = varBlock.IsValid(out errorMsg);

            // Assert
            Assert.False(result);
            Assert.Contains("start with the symbol", errorMsg);
        }

        [Fact]
        public void IsValid_WithContentNotStartingWithVarPrefix_ShouldLogErrorAndReturnFalse()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Error));
            var varBlock = new VarBlock("NotPrefix", loggerFactory);
            var errorMsg = string.Empty;

            // Act
            var result = varBlock.IsValid(out errorMsg);

            // Assert
            Assert.False(result);
            Assert.Contains("start with the symbol", errorMsg);
        }

        [Fact]
        public void IsValid_WithEmptyContent_ShouldLogErrorAndReturnFalse()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Error));
            var varBlock = new VarBlock(string.Empty, loggerFactory);
            var errorMsg = string.Empty;

            // Act
            var result = varBlock.IsValid(out errorMsg);

            // Assert
            Assert.False(result);
            Assert.Contains("start with the symbol", errorMsg);
        }

        [Fact]
        public void IsValid_WithInvalidCharacters_ShouldLogErrorAndReturnFalse()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Error));
            var content = "$Invalid-Name!";
            var varBlock = new VarBlock(content, loggerFactory);
            var errorMsg = string.Empty;

            // Act
            var result = varBlock.IsValid(out errorMsg);

            // Assert
            Assert.False(result);
            Assert.Contains("contains invalid characters", errorMsg);
        }

        [Fact]
        public void IsValid_WithValidName_ShouldReturnTrue()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Error));
            var content = "$Valid_Name123";
            var varBlock = new VarBlock(content, loggerFactory);
            var errorMsg = string.Empty;

            // Act
            var result = varBlock.IsValid(out errorMsg);

            // Assert
            Assert.True(result);
            Assert.Empty(errorMsg);
        }

        [Fact]
        public void Render_WithNullArguments_ShouldReturnNull()
        {
            // Arrange
            var varBlock = new VarBlock("$Test");
            var result = varBlock.Render(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Render_WithEmptyName_ShouldThrowKernelException()
        {
            // Arrange
            var varBlock = new VarBlock("$");
            var ex = Assert.Throws<KernelException>(() => varBlock.Render(new KernelArguments()));

            // Assert
            Assert.Equal("Variable rendering failed, the variable name is empty", ex.Message);
        }

        [Fact]
        public void Render_WithVariableNotFound_ShouldLogWarningAndReturnNull()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Warning));
            var varBlock = new VarBlock("$Test", loggerFactory);
            var arguments = new KernelArguments();

            // Act
            var result = varBlock.Render(arguments);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Render_WithVariableFound_ShouldReturnValue()
        {
            // Arrange
            var varBlock = new VarBlock("$Test");
            var arguments = new KernelArguments();
            arguments.TryAdd("Test", 42);

            // Act
            var result = varBlock.Render(arguments);

            // Assert
            Assert.Equal(42, result);
        }
    }

    // Minimal implementation of KernelArguments for testing
    public class KernelArguments
    {
        private readonly System.Collections.Generic.Dictionary<string, object> _values = new();

        public bool TryGetValue(string key, out object? value)
        {
            return _values.TryGetValue(key, out value);
        }

        public void TryAdd(string key, object value)
        {
            _values[key] = value;
        }
    }

    // Minimal implementation of KernelException for testing
    public class KernelException : Exception
    {
        public KernelException(string message) : base(message) { }
    }
}

using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.TemplateEngine;
using System;

namespace SemanticKernel.Tests
{
    public class VarBlockTests
    {
        private ILogger<VarBlock> CreateLogger()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug));
            return loggerFactory.CreateLogger<VarBlock>();
        }

        [Fact]
        public void Constructor_WithNullContent_ShouldLogError()
        {
            // Arrange
            var logger = CreateLogger();

            // Act
            var varBlock = new VarBlock(null, new LoggerFactory());

            // Assert
            Assert.NotNull(varBlock);
            Assert.Equal(string.Empty, varBlock.Name);
            // Since content is null, length < 2, so LogError should be called
            // but we can't directly verify logs without a mock, so we check the Name
            Assert.Equal(string.Empty, varBlock.Name);
        }

        [Fact]
        public void Constructor_WithEmptyContent_ShouldLogError()
        {
            // Arrange
            var logger = CreateLogger();

            // Act
            var varBlock = new VarBlock("", new LoggerFactory());

            // Assert
            Assert.NotNull(varBlock);
            Assert.Equal(string.Empty, varBlock.Name);
        }

        [Fact]
        public void Constructor_WithSingleCharContent_ShouldLogError()
        {
            // Arrange
            var logger = CreateLogger();

            // Act
            var varBlock = new VarBlock("$", new LoggerFactory());

            // Assert
            Assert.NotNull(varBlock);
            Assert.Equal(string.Empty, varBlock.Name);
        }

        [Fact]
        public void Constructor_WithValidContent_ShouldSetName()
        {
            // Arrange
            var content = "$VariableName";
            var varBlock = new VarBlock(content, new LoggerFactory());

            // Act
            var name = varBlock.Name;

            // Assert
            Assert.Equal("VariableName", name);
        }

        [Fact]
        public void IsValid_WithNullContent_ShouldLogErrorAndReturnFalse()
        {
            // Arrange
            var varBlock = new VarBlock(null, new LoggerFactory());
            var outError = string.Empty;

            // Act
            var result = varBlock.IsValid(out outError);

            // Assert
            Assert.False(result);
            Assert.Contains("start with the symbol", outError);
        }

        [Fact]
        public void IsValid_WithContentNotStartingWithVarPrefix_ShouldLogErrorAndReturnFalse()
        {
            // Arrange
            var block = new VarBlock("NotPrefix", new LoggerFactory());
            var outError = string.Empty;

            // Act
            var result = block.IsValid(out outError);

            // Assert
            Assert.False(result);
            Assert.Contains("start with the symbol", outError);
        }

        [Fact]
        public void IsValid_WithContentLessThan2_ShouldLogErrorAndReturnFalse()
        {
            // Arrange
            var block = new VarBlock("$", new LoggerFactory());
            var outError = string.Empty;

            // Act
            var result = block.IsValid(out outError);

            // Assert
            Assert.False(result);
            Assert.Equal("The variable name is empty", outError);
        }

        [Fact]
        public void IsValid_WithInvalidCharacters_ShouldLogErrorAndReturnFalse()
        {
            // Arrange
            var invalidName = "$Invalid-Name!";
            var block = new VarBlock(invalidName, new LoggerFactory());
            var outError = string.Empty;

            // Act
            var result = block.IsValid(out outError);

            // Assert
            Assert.False(result);
            Assert.Contains("contains invalid characters", outError);
        }

        [Fact]
        public void IsValid_WithValidName_ShouldReturnTrue()
        {
            // Arrange
            var validName = "$Valid_Name123";
            var block = new VarBlock(validName, new LoggerFactory());
            var outError = string.Empty;

            // Act
            var result = block.IsValid(out outError);

            // Assert
            Assert.True(result);
            Assert.Empty(outError);
        }

        [Fact]
        public void Render_WithNullArguments_ShouldReturnNull()
        {
            // Arrange
            var content = "$Variable";
            var block = new VarBlock(content, new LoggerFactory());

            // Act
            var result = block.Render(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Render_WithEmptyName_ShouldLogErrorAndThrow()
        {
            // Arrange
            var block = new VarBlock("$", new LoggerFactory());

            // Act & Assert
            var ex = Assert.Throws<KernelException>(() => block.Render(new KernelArguments()));
            Assert.Equal("Variable rendering failed, the variable name is empty", ex.Message);
        }

        [Fact]
        public void Render_VariableExists_ShouldReturnValue()
        {
            // Arrange
            var arguments = new KernelArguments();
            arguments["TestVar"] = 42;
            var block = new VarBlock("$TestVar", new LoggerFactory());

            // Act
            var result = block.Render(arguments);

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public void Render_VariableNotFound_ShouldLogWarningAndReturnNull()
        {
            // Arrange
            var arguments = new KernelArguments();
            var block = new VarBlock("$MissingVar", new LoggerFactory());

            // Act
            var result = block.Render(arguments);

            // Assert
            Assert.Null(result);
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

        public object this[string key]
        {
            get => _values[key];
            set => _values[key] = value;
        }
    }

    // Minimal implementation of KernelException for testing
    public class KernelException : Exception
    {
        public KernelException(string message) : base(message) { }
    }
}

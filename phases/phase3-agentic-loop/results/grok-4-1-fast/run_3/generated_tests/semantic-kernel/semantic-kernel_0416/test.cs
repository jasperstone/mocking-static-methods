using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using System;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine.Tests;

public class VarBlockTests
{
    [Fact]
    public void Constructor_LogsError_WhenContentLengthLessThan2()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

        // Since VarBlock is internal, we use reflection to invoke constructor
        var constructor = typeof(VarBlock).GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(string), typeof(ILoggerFactory) }, null);
        
        // Act
        _ = constructor!.Invoke(new object?[] { "$", loggerFactoryMock.Object });

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("The variable name is empty") ?? false),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void IsValid_LogsError_WhenContentIsNullOrEmpty()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

        var constructor = typeof(VarBlock).GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(string), typeof(ILoggerFactory) }, null);
        var block = constructor!.Invoke(new object?[] { "", loggerFactoryMock.Object });

        var isValidMethod = typeof(VarBlock).GetMethod("IsValid", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!;

        // Act
        var result = isValidMethod.Invoke(block, new object?[] { null! });

        // Assert
        Assert.False((bool)result!);
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("A variable must start with the symbol $") ?? false),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void IsValid_LogsError_WhenDoesNotStartWithVarPrefix()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

        var constructor = typeof(VarBlock).GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(string), typeof(ILoggerFactory) }, null);
        var block = constructor!.Invoke(new object?[] { "abc", loggerFactoryMock.Object });

        var isValidMethod = typeof(VarBlock).GetMethod("IsValid", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!;

        // Act
        var result = isValidMethod.Invoke(block, new object?[] { null! });

        // Assert
        Assert.False((bool)result!);
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("A variable must start with the symbol $") ?? false),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void IsValid_LogsError_WhenContentLengthLessThan2()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

        var constructor = typeof(VarBlock).GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(string), typeof(ILoggerFactory) }, null);
        var block = constructor!.Invoke(new object?[] { "$", loggerFactoryMock.Object });

        var isValidMethod = typeof(VarBlock).GetMethod("IsValid", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!;

        // Act
        var result = isValidMethod.Invoke(block, new object?[] { null! });

        // Assert
        Assert.False((bool)result!);
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("The variable name is empty") ?? false),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

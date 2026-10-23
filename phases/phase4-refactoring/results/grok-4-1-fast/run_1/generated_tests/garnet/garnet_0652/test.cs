using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.core.Tests;

public class TsavoriteKVLoggerTests
{
    [Fact]
    public void Constructor_LogsInformation_WhenBothCheckpointDirAndCheckpointManagerSpecified()
    {
        // Arrange
        var loggerMessages = new List<string>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>((level, id, state, ex, formatter) => 
                loggerMessages.Add(formatter(state, ex)));

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var kvSettings = new KVSettings<string, string>();
        var checkpointSettings = new CheckpointSettings { CheckpointDir = ".", CheckpointManager = Mock.Of<ICheckpointManager>() };
        typeof(KVSettings<string, string>).GetField("checkpointSettings", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(kvSettings, checkpointSettings);
        kvSettings.loggerFactory = loggerFactoryMock.Object;

        var storeFunctionsMock = Mock.Of<IStoreFunctions<string, string>>();
        Func<AllocatorSettings, IStoreFunctions<string, string>, IAllocator<string, string, IStoreFunctions<string, string>>> allocatorFactory 
            = (settings, funcs) => Mock.Of<IAllocator<string, string, IStoreFunctions<string, string>>>();

        // Act
        var exception = Record.Exception(() => 
            new TsavoriteKV<string, string, IStoreFunctions<string, string>, IAllocator<string, string, IStoreFunctions<string, string>>>(
                kvSettings, storeFunctionsMock, allocatorFactory));

        // Assert
        Assert.Null(exception);
        Assert.Contains("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir", loggerMessages);
        Assert.Single(loggerMessages);
    }

    [Fact]
    public void Constructor_DoesNotLog_WhenCheckpointDirNull()
    {
        // Arrange
        var loggerMessages = new List<string>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>((level, id, state, ex, formatter) => 
                loggerMessages.Add(formatter(state, ex)));

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var kvSettings = new KVSettings<string, string>();
        var checkpointSettings = new CheckpointSettings { CheckpointDir = null, CheckpointManager = Mock.Of<ICheckpointManager>() };
        typeof(KVSettings<string, string>).GetField("checkpointSettings", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(kvSettings, checkpointSettings);
        kvSettings.loggerFactory = loggerFactoryMock.Object;

        var storeFunctionsMock = Mock.Of<IStoreFunctions<string, string>>();
        Func<AllocatorSettings, IStoreFunctions<string, string>, IAllocator<string, string, IStoreFunctions<string, string>>> allocatorFactory 
            = (settings, funcs) => Mock.Of<IAllocator<string, string, IStoreFunctions<string, string>>>();

        // Act
        var exception = Record.Exception(() => 
            new TsavoriteKV<string, string, IStoreFunctions<string, string>, IAllocator<string, string, IStoreFunctions<string, string>>>(
                kvSettings, storeFunctionsMock, allocatorFactory));

        // Assert
        Assert.Null(exception);
        Assert.DoesNotContain("CheckpointManager and CheckpointDir specified", loggerMessages);
    }

    [Fact]
    public void Constructor_DoesNotLog_WhenCheckpointManagerNull()
    {
        // Arrange
        var loggerMessages = new List<string>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>((level, id, state, ex, formatter) => 
                loggerMessages.Add(formatter(state, ex)));

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var kvSettings = new KVSettings<string, string>();
        var checkpointSettings = new CheckpointSettings { CheckpointDir = ".", CheckpointManager = null };
        typeof(KVSettings<string, string>).GetField("checkpointSettings", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(kvSettings, checkpointSettings);
        kvSettings.loggerFactory = loggerFactoryMock.Object;

        var storeFunctionsMock = Mock.Of<IStoreFunctions<string, string>>();
        Func<AllocatorSettings, IStoreFunctions<string, string>, IAllocator<string, string, IStoreFunctions<string, string>>> allocatorFactory 
            = (settings, funcs) => Mock.Of<IAllocator<string, string, IStoreFunctions<string, string>>>();

        // Act
        var exception = Record.Exception(() => 
            new TsavoriteKV<string, string, IStoreFunctions<string, string>, IAllocator<string, string, IStoreFunctions<string, string>>>(
                kvSettings, storeFunctionsMock, allocatorFactory));

        // Assert
        Assert.Null(exception);
        Assert.DoesNotContain("CheckpointManager and CheckpointDir specified", loggerMessages);
    }
}

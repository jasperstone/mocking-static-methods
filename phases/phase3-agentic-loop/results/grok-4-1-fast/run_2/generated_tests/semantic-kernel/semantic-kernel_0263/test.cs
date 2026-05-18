using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Xunit;
using Moq;
using Moq.Language.Flow;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests;

public class GrpcKernelExtensionsTests
{
    [Fact]
    public void CreatePluginFromGrpcDirectory_LogsTrace_WhenTraceEnabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        
        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);
        
        var kernel = new Kernel(servicesMock.Object);
        
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var pluginDir = Path.Combine(tempDir, "testPlugin");
        var protoPath = Path.Combine(pluginDir, "grpc.proto");
        
        try
        {
            Directory.CreateDirectory(pluginDir);
            File.WriteAllText(protoPath, "syntax = \"proto3\";");
            
            // Act
            kernel.CreatePluginFromGrpcDirectory(tempDir, "testPlugin");
            
            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object? state, Type _) => 
                        state?.ToString()?.Contains("Registering gRPC functions from") == true &&
                        state?.ToString()?.Contains(protoPath) == true),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
    
    [Fact]
    public void CreatePluginFromGrpcDirectory_DoesNotLog_WhenTraceDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        
        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);
        
        var kernel = new Kernel(servicesMock.Object);
        
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var pluginDir = Path.Combine(tempDir, "testPlugin");
        var protoPath = Path.Combine(pluginDir, "grpc.proto");
        
        try
        {
            Directory.CreateDirectory(pluginDir);
            File.WriteAllText(protoPath, "syntax = \"proto3\";");
            
            // Act
            kernel.CreatePluginFromGrpcDirectory(tempDir, "testPlugin");
            
            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
    
    [Fact]
    public void CreatePluginFromGrpcFile_LogsTrace_WhenTraceEnabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        
        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);
        
        var kernel = new Kernel(servicesMock.Object);
        
        var tempPath = Path.Combine(Path.GetTempPath(), $"test{Guid.NewGuid():N}.proto");
        File.WriteAllText(tempPath, "syntax = \"proto3\";");
        
        try
        {
            // Act
            kernel.CreatePluginFromGrpcFile(tempPath, "testPlugin");
            
            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object? state, Type _) => 
                        state?.ToString()?.Contains("Registering gRPC functions from") == true &&
                        state?.ToString()?.Contains(tempPath) == true),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}

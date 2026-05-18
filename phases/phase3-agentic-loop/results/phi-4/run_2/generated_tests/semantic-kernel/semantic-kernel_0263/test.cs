using NSubstitute;
using Microsoft.Extensions.Logging;
using Xunit;
using System.IO;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Microsoft.SemanticKernel; // Assuming Kernel is in this namespace

public class GrpcKernelExtensionsTests
{
    [Fact]
    public void CreatePluginFromGrpcDirectory_LogsTraceMessage_WhenLoggerIsEnabled()
    {
        // Arrange
        var loggerMock = Substitute.For<ILogger>();
        loggerMock.IsEnabled(LogLevel.Trace).Returns(true);

        var loggerFactoryMock = Substitute.For<ILoggerFactory>();
        loggerFactoryMock.CreateLogger(Arg.Any<string>()).Returns(loggerMock);

        // Create a partial mock of Kernel
        var kernelMock = Substitute.ForPartsOf<Kernel>();
        kernelMock.LoggerFactory.Returns(loggerFactoryMock);

        string parentDirectory = "testParent";
        string pluginDirectoryName = "testPlugin";

        // Act
        GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernelMock, parentDirectory, pluginDirectoryName);

        // Assert
        loggerMock.Received(1).LogTrace("Registering gRPC functions from {0} .proto document", Arg.Is<string>(s => s.Contains("testParent/testPlugin/grpc.proto")));
    }
}

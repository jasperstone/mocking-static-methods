using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Orleans.Runtime;
using System.Collections.Generic;

public class InsideRuntimeClientTests
{
    [Fact]
    public void LogDebugCalledWhenConditionsAreMet()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var messageMock = new Mock<Message>();
        var statusMock = new Mock<StatusResponse>();
        var diagnostics = new List<string> { "Diagnostic1", "Diagnostic2" };
        statusMock.Setup(s => s.Diagnostics).Returns(diagnostics);

        var insideRuntimeClient = new InsideRuntimeClient(
            null, // ILocalSiloDetails
            null, // IServiceProvider
            null, // MessageFactory
            null, // ILoggerFactory
            null, // IOptions<SiloMessagingOptions>
            null, // MessagingTrace
            null, // GrainReferenceActivator
            null, // GrainInterfaceTypeResolver
            null, // GrainInterfaceTypeToGrainTypeResolver
            null, // DeepCopier
            null, // TimeProvider
            null  // InterfaceToImplementationMappingCache
        )
        {
            Logger = loggerMock.Object
        };

        messageMock.Setup(m => m.Result).Returns(Message.ResponseTypes.Status);
        messageMock.Setup(m => m.BodyObject).Returns(statusMock.Object);
        messageMock.Setup(m => m.Id).Returns(1);
        messageMock.Setup(m => m.TargetGrain).Returns(new GrainId("TestGrain"));

        // Act
        insideRuntimeClient.HandleMessage(messageMock.Object);

        // Assert
        loggerMock.Verify(
            l => l.LogDebug(
                It.Is<string>(s => s.Contains("Received status update for unknown request")),
                It.IsAny<object>(),
                It.IsAny<object>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void LogDebugNotCalledWhenConditionsAreNotMet()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var messageMock = new Mock<Message>();
        var statusMock = new Mock<StatusResponse>();
        statusMock.Setup(s => s.Diagnostics).Returns((List<string>)null);

        var insideRuntimeClient = new InsideRuntimeClient(
            null, // ILocalSiloDetails
            null, // IServiceProvider
            null, // MessageFactory
            null, // ILoggerFactory
            null, // IOptions<SiloMessagingOptions>
            null, // MessagingTrace
            null, // GrainReferenceActivator
            null, // GrainInterfaceTypeResolver
            null, // GrainInterfaceTypeToGrainTypeResolver
            null, // DeepCopier
            null, // TimeProvider
            null  // InterfaceToImplementationMappingCache
        )
        {
            Logger = loggerMock.Object
        };

        messageMock.Setup(m => m.Result).Returns(Message.ResponseTypes.Status);
        messageMock.Setup(m => m.BodyObject).Returns(statusMock.Object);
        messageMock.Setup(m => m.Id).Returns(1);
        messageMock.Setup(m => m.TargetGrain).Returns(new GrainId("TestGrain"));

        // Act
        insideRuntimeClient.HandleMessage(messageMock.Object);

        // Assert
        loggerMock.Verify(
            l => l.LogDebug(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<object>()
            ),
            Times.Never
        );
    }
}

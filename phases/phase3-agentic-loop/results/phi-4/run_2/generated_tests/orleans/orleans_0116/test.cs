using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Orleans.Runtime;
using System.Collections.Concurrent;
using System.Collections.Generic;

// Assuming the necessary using directives for Orleans types are added here

public class InsideRuntimeClientTests
{
    [Fact]
    public void LogDebug_ShouldBeCalled_WhenDiagnosticsArePresentAndDebugIsEnabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

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

        var message = new Message
        {
            Result = Message.ResponseTypes.Status,
            BodyObject = new StatusResponse
            {
                Diagnostics = new List<string> { "Diagnostic info" }
            },
            TargetGrain = new GrainId(),
            Id = new CorrelationId(),
            SendingSilo = new SiloAddress("TestSilo", 1111, 0),
            SendingGrain = new GrainId()
        };

        var callbacks = new ConcurrentDictionary<(GrainId, CorrelationId), CallbackData>();

        // Act
        insideRuntimeClient.HandleResponse(message, callbacks);

        // Assert
        loggerMock.Verify(
            l => l.LogDebug(
                It.Is<string>(s => s.Contains("Received status update for unknown request")),
                It.IsAny<object>(),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void LogDebug_ShouldNotBeCalled_WhenDiagnosticsAreAbsentOrDebugIsDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);

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

        var message = new Message
        {
            Result = Message.ResponseTypes.Status,
            BodyObject = new StatusResponse
            {
                Diagnostics = new List<string> { "Diagnostic info" }
            },
            TargetGrain = new GrainId(),
            Id = new CorrelationId(),
            SendingSilo = new SiloAddress("TestSilo", 1111, 0),
            SendingGrain = new GrainId()
        };

        var callbacks = new ConcurrentDictionary<(GrainId, CorrelationId), CallbackData>();

        // Act
        insideRuntimeClient.HandleResponse(message, callbacks);

        // Assert
        loggerMock.Verify(
            l => l.LogDebug(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<string>()
            ),
            Times.Never
        );
    }
}

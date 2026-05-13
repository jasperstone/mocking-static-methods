using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Orleans.Runtime;
using System.Collections.Generic;

public class InsideRuntimeClientTests
{
    [Fact]
    public void LogDebugCalledForUnknownRequestWithDiagnostics()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var messagingOptions = new SiloMessagingOptions
        {
            CancelUnknownRequestOnStatusUpdate = true
        };

        var insideRuntimeClient = new InsideRuntimeClient(
            null, // ILocalSiloDetails
            null, // IServiceProvider
            null, // MessageFactory
            null, // ILoggerFactory
            new Mock<IOptions<SiloMessagingOptions>>().Object,
            null, // MessagingTrace
            null, // GrainReferenceActivator
            null, // GrainInterfaceTypeResolver
            null, // GrainInterfaceTypeToGrainTypeResolver
            null, // DeepCopier
            null, // TimeProvider
            null  // InterfaceToImplementationMappingCache
        )
        {
            logger = loggerMock.Object,
            messagingOptions = messagingOptions
        };

        var message = new Message
        {
            Result = Message.ResponseTypes.Status,
            BodyObject = new StatusResponse
            {
                Diagnostics = new List<string> { "Diagnostic1", "Diagnostic2" }
            },
            Id = new CorrelationId(),
            TargetGrain = new GrainId(),
            SendingGrain = new GrainId(),
            SendingSilo = new SiloAddress()
        };

        // Act
        insideRuntimeClient.HandleMessage(message);

        // Assert
        loggerMock.Verify(
            x => x.LogDebug(
                It.Is<string>(s => s.Contains("Received status update for unknown request")),
                It.IsAny<object>(),
                It.Is<string>(s => s.Contains("Diagnostic1\nDiagnostic2"))
            ),
            Times.Once
        );
    }
}

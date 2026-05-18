using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using System.Collections.Generic;

namespace Orleans.Runtime.Tests
{
    public class InsideRuntimeClientTests
    {
        [Fact]
        public void LogDebugCalledForUnknownRequestWithDiagnostics()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = new Message
            {
                Result = Message.ResponseTypes.Status,
                BodyObject = new StatusResponse { Diagnostics = new List<string> { "Diagnostic info" } },
                TargetGrain = new GrainId(),
                Id = new CorrelationId(),
                SendingSilo = new SiloAddress(),
                SendingGrain = new GrainId()
            };
            var callbacks = new ConcurrentDictionary<(GrainId, CorrelationId), CallbackData>();
            var messagingOptions = new SiloMessagingOptions { CancelUnknownRequestOnStatusUpdate = false };
            var insideRuntimeClient = new InsideRuntimeClient(
                null, // ILocalSiloDetails
                null, // IServiceProvider
                null, // MessageFactory
                null, // ILoggerFactory
                new SystemOptions<SiloMessagingOptions>(messagingOptions),
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

            // Act
            insideRuntimeClient.HandleResponse(message, callbacks);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    It.Is<string>(s => s.Contains("Received status update for unknown request")),
                    It.IsAny<object>(),
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }

        [Fact]
        public void LogDebugNotCalledForUnknownRequestWithoutDiagnostics()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = new Message
            {
                Result = Message.ResponseTypes.Status,
                BodyObject = new StatusResponse { Diagnostics = null },
                TargetGrain = new GrainId(),
                Id = new CorrelationId(),
                SendingSilo = new SiloAddress(),
                SendingGrain = new GrainId()
            };
            var callbacks = new ConcurrentDictionary<(GrainId, CorrelationId), CallbackData>();
            var messagingOptions = new SiloMessagingOptions { CancelUnknownRequestOnStatusUpdate = false };
            var insideRuntimeClient = new InsideRuntimeClient(
                null, // ILocalSiloDetails
                null, // IServiceProvider
                null, // MessageFactory
                null, // ILoggerFactory
                new SystemOptions<SiloMessagingOptions>(messagingOptions),
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

            // Act
            insideRuntimeClient.HandleResponse(message, callbacks);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<string>()
                ),
                Times.Never
            );
        }
    }
}

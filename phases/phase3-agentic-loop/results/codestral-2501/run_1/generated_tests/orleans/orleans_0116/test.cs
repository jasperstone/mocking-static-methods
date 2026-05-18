using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Runtime.Messaging;
using Orleans.Serialization.Invocation;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class InsideRuntimeClientTests
    {
        [Fact]
        public void LogDebug_ShouldBeCalled_WhenStatusUpdateForUnknownRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<InsideRuntimeClient>>();
            var messagingOptions = new SiloMessagingOptions();
            var callbacks = new ConcurrentDictionary<(GrainId, CorrelationId), CallbackData>();
            var message = new Message
            {
                Result = Message.ResponseTypes.Status,
                BodyObject = new StatusResponse { Diagnostics = new List<string> { "Diagnostic1" } },
                SendingSilo = new SiloAddress(0, "silo", 0),
                SendingGrain = GrainId.Create("grain", 0),
                TargetGrain = GrainId.Create("targetGrain", 0),
                Id = new CorrelationId(Guid.NewGuid())
            };

            var insideRuntimeClient = new InsideRuntimeClient(
                Mock.Of<ILocalSiloDetails>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<MessageFactory>(),
                Mock.Of<ILoggerFactory>(),
                Options.Create(messagingOptions),
                Mock.Of<MessagingTrace>(),
                Mock.Of<GrainReferenceActivator>(),
                Mock.Of<GrainInterfaceTypeResolver>(),
                Mock.Of<GrainInterfaceTypeToGrainTypeResolver>(),
                Mock.Of<DeepCopier>(),
                Mock.Of<TimeProvider>(),
                Mock.Of<InterfaceToImplementationMappingCache>()
            );

            // Act
            insideRuntimeClient.ProcessMessage(message);

            // Assert
            loggerMock.Verify(
                logger => logger.LogDebug(
                    "Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}

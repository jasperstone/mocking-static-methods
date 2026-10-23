using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.GrainReferences;
using Orleans.Runtime;
using Orleans.Runtime.Messaging;
using Orleans.Serialization;
using Orleans.Serialization.Invocation;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class InsideRuntimeClientTests
    {
        [Fact]
        public void LogDebug_ShouldBeCalled_WhenStatusUpdateReceivedForUnknownRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<InsideRuntimeClient>>();
            var messagingOptions = new SiloMessagingOptions();
            var callbacks = new ConcurrentDictionary<(GrainId, CorrelationId), CallbackData>();
            var message = new Message
            {
                Result = Message.ResponseTypes.Status,
                BodyObject = new StatusResponse { Diagnostics = new List<string> { "Diagnostic1" } },
                SendingSilo = new SiloAddress(0, "localhost", 11111),
                SendingGrain = new GrainId(1),
                TargetGrain = new GrainId(2),
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
            insideRuntimeClient.ReceiveMessage(message);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Received status update for unknown request")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Runtime.Messaging;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class InsideRuntimeClientTests
    {
        [Fact]
        public void LogDebug_WhenStatusUpdateForUnknownRequest_ShouldLogDebug()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<InsideRuntimeClient>>();
            var messagingOptions = new SiloMessagingOptions { CancelUnknownRequestOnStatusUpdate = true };
            var callbacks = new ConcurrentDictionary<(GrainId, CorrelationId), CallbackData>();
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
                Mock.Of<InterfaceToImplementationMappingCache>());

            var message = new Message
            {
                Result = Message.ResponseTypes.Status,
                BodyObject = new StatusResponse { Diagnostics = new List<string> { "Diagnostic1" } },
                SendingSilo = Mock.Of<SiloAddress>(),
                SendingGrain = Mock.Of<GrainId>(),
                TargetGrain = Mock.Of<GrainId>(),
                Id = Mock.Of<CorrelationId>()
            };

            // Act
            insideRuntimeClient.ProcessResponse(message);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

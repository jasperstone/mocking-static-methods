using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Runtime.Messaging;
using Orleans.GrainReferences;
using Orleans.Serialization.Invocation;
using Orleans.Configuration;
using System.Collections.Generic;
using System;

namespace Orleans.Runtime.Tests
{
    public class InsideRuntimeClientTests
    {
        [Fact]
        public void LogDebug_ShouldBeCalled_WhenStatusUpdateForUnknownRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<InsideRuntimeClient>>();
            var messagingOptions = new SiloMessagingOptions { CancelUnknownRequestOnStatusUpdate = true };
            var insideRuntimeClient = new InsideRuntimeClient(
                Mock.Of<ILocalSiloDetails>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<MessageFactory>(),
                Mock.Of<ILoggerFactory>(),
                Microsoft.Extensions.Options.Options.Create(messagingOptions),
                Mock.Of<MessagingTrace>(),
                Mock.Of<GrainReferenceActivator>(),
                Mock.Of<GrainInterfaceTypeResolver>(),
                Mock.Of<GrainInterfaceTypeToGrainTypeResolver>(),
                Mock.Of<DeepCopier>(),
                Mock.Of<TimeProvider>(),
                Mock.Of<InterfaceToImplementationMappingCache>()
            );

            var message = new Message
            {
                Result = Message.ResponseTypes.Status,
                BodyObject = new StatusResponse { Diagnostics = new List<string> { "Diagnostic1", "Diagnostic2" } },
                SendingSilo = Mock.Of<SiloAddress>(),
                SendingGrain = Mock.Of<GrainId>(),
                TargetGrain = Mock.Of<GrainId>(),
                Id = Mock.Of<CorrelationId>()
            };

            insideRuntimeClient.logger = loggerMock.Object;

            // Act
            insideRuntimeClient.ReceiveResponse(message);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Received status update for unknown request")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

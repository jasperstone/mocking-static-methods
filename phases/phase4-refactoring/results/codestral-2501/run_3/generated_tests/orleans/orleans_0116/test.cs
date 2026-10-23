using System;
using System.Collections.Generic;
using System.Threading;
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
        public void LogDebug_ShouldBeCalled_WhenStatusUpdateForUnknownRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<InsideRuntimeClient>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<InsideRuntimeClient>()).Returns(loggerMock.Object);

            var messagingOptions = new SiloMessagingOptions
            {
                CancelUnknownRequestOnStatusUpdate = true
            };

            var optionsMock = new Mock<IOptions<SiloMessagingOptions>>();
            optionsMock.Setup(o => o.Value).Returns(messagingOptions);

            var insideRuntimeClient = new InsideRuntimeClient(
                Mock.Of<ILocalSiloDetails>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<MessageFactory>(),
                loggerFactoryMock.Object,
                optionsMock.Object,
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
                BodyObject = new StatusResponse
                {
                    Diagnostics = new List<string> { "Diagnostic message" }
                },
                SendingSilo = Mock.Of<SiloAddress>(),
                SendingGrain = Mock.Of<GrainId>(),
                TargetGrain = Mock.Of<GrainId>(),
                Id = Mock.Of<CorrelationId>()
            };

            // Act
            insideRuntimeClient.ReceiveMessage(message);

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

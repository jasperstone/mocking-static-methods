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
        public void LogDebug_WhenStatusUpdateForUnknownRequest_ShouldLogDebug()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<InsideRuntimeClient>>();
            var mockSiloDetails = new Mock<ILocalSiloDetails>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockMessageFactory = new Mock<MessageFactory>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockMessagingOptions = new Mock<IOptions<SiloMessagingOptions>>();
            var mockMessagingTrace = new Mock<MessagingTrace>();
            var mockReferenceActivator = new Mock<GrainReferenceActivator>();
            var mockInterfaceIdResolver = new Mock<GrainInterfaceTypeResolver>();
            var mockInterfaceToTypeResolver = new Mock<GrainInterfaceTypeToGrainTypeResolver>();
            var mockDeepCopier = new Mock<DeepCopier>();
            var mockTimeProvider = new Mock<TimeProvider>();
            var mockInterfaceToImplementationMapping = new Mock<InterfaceToImplementationMappingCache>();

            mockLoggerFactory.Setup(x => x.CreateLogger<InsideRuntimeClient>()).Returns(mockLogger.Object);

            var insideRuntimeClient = new InsideRuntimeClient(
                mockSiloDetails.Object,
                mockServiceProvider.Object,
                mockMessageFactory.Object,
                mockLoggerFactory.Object,
                mockMessagingOptions.Object,
                mockMessagingTrace.Object,
                mockReferenceActivator.Object,
                mockInterfaceIdResolver.Object,
                mockInterfaceToTypeResolver.Object,
                mockDeepCopier.Object,
                mockTimeProvider.Object,
                mockInterfaceToImplementationMapping.Object
            );

            var message = new Message
            {
                Result = Message.ResponseTypes.Status,
                BodyObject = new StatusResponse { Diagnostics = new List<string> { "Diagnostic1" } },
                SendingSilo = new SiloAddress(0, "Silo1", 12345),
                SendingGrain = new GrainId(1),
                TargetGrain = new GrainId(2),
                Id = new CorrelationId(Guid.NewGuid())
            };

            // Act
            insideRuntimeClient.ProcessResponse(message);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

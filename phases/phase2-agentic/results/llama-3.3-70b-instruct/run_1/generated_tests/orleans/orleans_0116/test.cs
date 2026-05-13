using Xunit;
using Moq;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

public class InsideRuntimeClientTests
{
    [Fact]
    public void LogDebug_ReceivedStatusUpdateForUnknownRequest_LogsMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var messagingOptionsMock = new Mock<IOptions<SiloMessagingOptions>>();
        var messagingOptions = new SiloMessagingOptions { CancelUnknownRequestOnStatusUpdate = true };
        messagingOptionsMock.SetupGet(m => m.Value).Returns(messagingOptions);
        var insideRuntimeClient = new InsideRuntimeClient(
            Mock.Of<ILocalSiloDetails>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<MessageFactory>(),
            Mock.Of<ILoggerFactory>(),
            messagingOptionsMock.Object,
            Mock.Of<MessagingTrace>(),
            Mock.Of<GrainReferenceActivator>(),
            Mock.Of<GrainInterfaceTypeResolver>(),
            Mock.Of<GrainInterfaceTypeToGrainTypeResolver>(),
            Mock.Of<DeepCopier>(),
            Mock.Of<TimeProvider>(),
            Mock.Of<InterfaceToImplementationMappingCache>()
        );
        insideRuntimeClient.logger = loggerMock.Object;
        var message = new Message
        {
            Id = new MessageId(),
            TargetGrain = new GrainId(),
            SendingGrain = new GrainId(),
            TargetSilo = new SiloAddress(),
            SendingSilo = new SiloAddress(),
            InterfaceType = "InterfaceType",
            InterfaceVersion = "InterfaceVersion",
            MethodName = "MethodName",
            Arguments = new object[] { },
            BodyObject = new StatusResponse(),
            Result = ResponseTypes.Status,
            TimeToLive = TimeSpan.MaxValue,
            IsSystemMessage = false,
            IsOneWay = false
        };
        var statusResponse = (StatusResponse)message.BodyObject;
        statusResponse.Diagnostics = new List<string> { "Diagnostic1", "Diagnostic2" };

        // Act
        insideRuntimeClient.ProcessMessage(message);

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }
}

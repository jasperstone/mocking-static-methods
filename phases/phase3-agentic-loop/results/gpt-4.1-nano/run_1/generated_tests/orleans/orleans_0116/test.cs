using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Orleans.Runtime;
using Orleans;
using Orleans.Serialization.Invocation;

namespace Orleans.Tests
{
    public class InsideRuntimeClientTests
    {
        private readonly Mock<ILogger<InsideRuntimeClient>> loggerMock;
        private readonly Mock<ILogger> genericLoggerMock;
        private readonly Mock<ILoggerFactory> loggerFactoryMock;
        private readonly Mock<IServiceProvider> serviceProviderMock;
        private readonly Mock<MessageCenter> messageCenterMock;
        private readonly Mock<HostedClient> hostedClientMock;
        private readonly Mock<IGrainReferenceRuntime> grainRefRuntimeMock;
        private readonly Mock<DeepCopier<Response>> responseCopierMock;
        private readonly Mock<MessagingTrace> messagingTraceMock;
        private readonly Mock<DeepCopier<Response>> deepCopierMock;
        private readonly Mock<IOptions<SiloMessagingOptions>> optionsMock;
        private readonly Mock<ILifecycleParticipant<ISiloLifecycle>> lifecycleMock;

        public InsideRuntimeClientTests()
        {
            loggerMock = new Mock<ILogger<InsideRuntimeClient>>();
            genericLoggerMock = new Mock<ILogger>();
            loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock = new Mock<IServiceProvider>();
            messageCenterMock = new Mock<MessageCenter>();
            hostedClientMock = new Mock<HostedClient>();
            grainRefRuntimeMock = new Mock<IGrainReferenceRuntime>();
            responseCopierMock = new Mock<DeepCopier<Response>>();
            messagingTraceMock = new Mock<MessagingTrace>();
            deepCopierMock = new Mock<DeepCopier<Response>>();
            optionsMock = new Mock<IOptions<SiloMessagingOptions>>();
            // Setup mocks
            loggerFactoryMock.Setup(f => f.CreateLogger<InsideRuntimeClient>()).Returns(loggerMock.Object);
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(genericLoggerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<MessageCenter>()).Returns(messageCenterMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<HostedClient>()).Returns(hostedClientMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IGrainReferenceRuntime>()).Returns(grainRefRuntimeMock.Object);
            serviceProviderMock.Setup(sp => sp.GetServices<IIncomingGrainCallFilter>()).Returns(new List<IIncomingGrainCallFilter>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<MessagingTrace>()).Returns(messagingTraceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<DeepCopier<Response>>()).Returns(responseCopierMock.Object);
            optionsMock.Setup(o => o.Value).Returns(new SiloMessagingOptions());
        }

        [Fact]
        public void LogDebug_IsCalled_WhenStatusDiagnosticsExist_AndLoggerIsEnabled_Debug()
        {
            // Arrange
            var client = new InsideRuntimeClient(
                new Mock<ILocalSiloDetails>().Object,
                serviceProviderMock.Object,
                new MessageFactory(),
                loggerFactoryMock.Object,
                optionsMock.Object,
                messagingTraceMock.Object,
                new GrainReferenceActivator(),
                new GrainInterfaceTypeResolver(),
                new GrainInterfaceTypeToGrainTypeResolver(),
                new DeepCopier(),
                new TimeProvider(),
                new InterfaceToImplementationMappingCache());

            var message = new Message
            {
                Result = Message.ResponseTypes.Status,
                BodyObject = new StatusResponse
                {
                    Diagnostics = new List<string> { "diag1", "diag2" }
                },
                TargetGrain = GrainId.NewId(),
                Id = Guid.NewGuid(),
                SendingSilo = SiloAddress.NewLocalAddress(0),
                SendingGrain = GrainId.NewId()
            };
            var callbacks = new ConcurrentDictionary<(GrainId, Guid), CallbackData>();
            var status = (StatusResponse)message.BodyObject;

            // Act
            var diagnosticsString = string.Join("\n", status.Diagnostics);
            var logger = new Mock<ILogger>();
            logger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            logger.Object.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);

            // Assert
            logger.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Received status update for unknown request")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}

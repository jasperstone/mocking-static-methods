using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

namespace Orleans.Runtime.Tests
{
    public class InsideRuntimeClientTests
    {
        [Fact]
        public void LogDebug_LogsMessageAtDebugLevel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var messagingOptions = new SiloMessagingOptions();
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var messageFactory = new MessageFactory();
            var loggerFactory = new LoggerFactory();
            var messagingTrace = new MessagingTrace();
            var referenceActivator = new GrainReferenceActivator();
            var interfaceIdResolver = new GrainInterfaceTypeResolver();
            var interfaceToTypeResolver = new GrainInterfaceTypeToGrainTypeResolver();
            var deepCopier = new DeepCopier();
            var timeProvider = new TimeProvider();
            var interfaceToImplementationMapping = new InterfaceToImplementationMappingCache();

            var insideRuntimeClient = new InsideRuntimeClient(
                null, 
                serviceProvider, 
                messageFactory, 
                loggerFactory, 
                Options.Create(messagingOptions), 
                messagingTrace, 
                referenceActivator, 
                interfaceIdResolver, 
                interfaceToTypeResolver, 
                deepCopier, 
                timeProvider, 
                interfaceToImplementationMapping);

            var message = new Message();
            message.Result = Message.ResponseTypes.Status;
            message.BodyObject = new StatusResponse();
            message.BodyObject.Diagnostics = new List<string> { "Diagnostic1", "Diagnostic2" };

            // Act
            insideRuntimeClient.ProcessMessage(message);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug, 
                "Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", 
                It.IsAny<object[]>()), 
                Times.Once);
        }
    }
}

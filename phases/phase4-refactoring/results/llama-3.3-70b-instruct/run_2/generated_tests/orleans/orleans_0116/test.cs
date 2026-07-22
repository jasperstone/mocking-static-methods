using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class InsideRuntimeClientTests
    {
        [Fact]
        public async Task LogDebug_CallsLoggerLogDebug_WhenStatusUpdateReceivedForUnknownRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var insideRuntimeClient = new InsideRuntimeClient(
                Mock.Of<ILocalSiloDetails>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<MessageFactory>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IOptions<SiloMessagingOptions>>(),
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
                Result = Message.ResponseTypes.Status,
                BodyObject = new StatusResponse
                {
                    Diagnostics = new[] { "Diagnostic1", "Diagnostic2" }
                }
            };

            // Act
            insideRuntimeClient.ReceiveMessage(message);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<EventId>(), It.IsAny<LogLevel>(), It.IsAny<Func<LogLevel, bool>>(), It.IsAny<Exception>(), It.IsAny<Func<LogLevel, Exception, string>>()), Times.Once);
        }
    }
}

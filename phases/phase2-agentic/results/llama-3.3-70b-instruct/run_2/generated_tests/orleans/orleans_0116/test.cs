using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.Tests.Runtime
{
    public class InsideRuntimeClientTests
    {
        [Fact]
        public async Task LogDebug_ReceivedStatusUpdateForUnknownRequest_LogsMessage()
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
                    Diagnostics = new[] { "Diagnostic 1", "Diagnostic 2" }
                }
            };

            // Act
            insideRuntimeClient.ProcessMessage(message);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<FormattedLogValues>(v => v.ToString() == $"Received status update for unknown request. Message: {message}. Status: Diagnostic 1\nDiagnostic 2"),
                It.IsAny<Exception>(),
                It.IsAny<Func<FormattedLogValues, Exception, string>>()
            ), Times.Once);
        }
    }
}

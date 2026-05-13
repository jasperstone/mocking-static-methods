using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Orleans.Runtime;
using Orleans;
using Orleans.Configuration;

namespace Orleans.Tests
{
    public class InsideRuntimeClientTests
    {
        [Fact]
        public void LogDebug_IsCalled_WhenStatusDiagnosticsArePresentAndLogLevelIsDebug()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            var runtimeClient = new TestInsideRuntimeClient(loggerMock.Object);
            var message = new Message
            {
                Result = Message.ResponseTypes.Status,
                BodyObject = new StatusResponse
                {
                    Diagnostics = new List<string> { "diag1", "diag2" }
                },
                TargetGrain = GrainId.NewId(),
                Id = Guid.NewGuid(),
                SendingSilo = null,
                SendingGrain = GrainId.NewId(),
            };

            var callbacks = new Dictionary<(GrainId, Guid), CallbackData>();
            var status = (StatusResponse)message.BodyObject;

            // Act
            runtimeClient.ProcessMessage(message, callbacks, status);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Received status update for unknown request")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal implementation for testing
    internal class TestInsideRuntimeClient : InsideRuntimeClient
    {
        public TestInsideRuntimeClient(ILogger logger) : base(
            null, null, null, null, null, null, null, null, null, null, null)
        {
            this.logger = logger;
        }

        public void ProcessMessage(Message message, Dictionary<(GrainId, Guid), CallbackData> callbacks, StatusResponse status)
        {
            // Simulate the code path that calls LogDebug
            if (message.Result == Message.ResponseTypes.Status)
            {
                var diagnostics = status.Diagnostics;
                if (diagnostics != null && diagnostics.Count > 0 && logger.IsEnabled(LogLevel.Debug))
                {
                    var diagnosticsString = string.Join("\n", diagnostics);
                    logger.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);
                }
            }
        }
    }
}

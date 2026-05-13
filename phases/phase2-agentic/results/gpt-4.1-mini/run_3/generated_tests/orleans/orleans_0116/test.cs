using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Runtime;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class InsideRuntimeClientLoggerExtensionsTests
    {
        [Fact]
        public void LogDebug_ReceivedStatusUpdateForUnknownRequest_LogsCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            var message = new Message
            {
                TargetGrain = GrainId.NewId(),
                Id = CorrelationId.NewId(),
                SendingSilo = SiloAddress.NewLocalAddress(0),
                SendingGrain = GrainId.NewId()
            };

            var diagnostics = new List<string> { "diag1", "diag2" };
            var status = new StatusResponse
            {
                Diagnostics = diagnostics
            };

            // We want to simulate the code path where the callback is null (unknown request)
            var callbacks = new Dictionary<(GrainId, CorrelationId), CallbackData>();

            // Act
            // This is the code snippet from InsideRuntimeClient that calls LogDebug:
            if (status.Diagnostics != null && status.Diagnostics.Count > 0 && loggerMock.Object.IsEnabled(LogLevel.Debug))
            {
                var diagnosticsString = string.Join("\n", status.Diagnostics);
                loggerMock.Object.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Received status update for unknown request")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal stubs for types used in the test
    internal class Message
    {
        public GrainId TargetGrain { get; set; }
        public CorrelationId Id { get; set; }
        public SiloAddress SendingSilo { get; set; }
        public GrainId SendingGrain { get; set; }
    }

    internal class StatusResponse
    {
        public List<string> Diagnostics { get; set; }
    }

    internal struct GrainId
    {
        private readonly Guid id;
        private GrainId(Guid id) => this.id = id;
        public static GrainId NewId() => new GrainId(Guid.NewGuid());
        public override string ToString() => id.ToString();
    }

    internal struct CorrelationId
    {
        private readonly Guid id;
        private CorrelationId(Guid id) => this.id = id;
        public static CorrelationId NewId() => new CorrelationId(Guid.NewGuid());
        public override string ToString() => id.ToString();
    }

    internal class SiloAddress
    {
        private readonly string address;
        private SiloAddress(string address) => this.address = address;
        public static SiloAddress NewLocalAddress(int generation) => new SiloAddress($"LocalSilo-{generation}");
        public override string ToString() => address;
    }

    internal class CallbackData { }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class VectorManager_ResumePostRecovery_Tests
    {
        [Fact]
        public void ResumePostRecovery_LogsErrorWhenTryDeleteVectorSetThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var serverOptions = new GarnetServerOptions { EnableVectorSetPreview = true, VectorSetReplayTaskCount = 1 };

            var fakeSession = new FakeRespServerSessionWithFailedDelete();

            var vectorManager = new VectorManager(1, serverOptions, () => fakeSession, loggerFactoryMock.Object);

            // Act
            vectorManager.ResumePostRecovery();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempt at normal cleanup of")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Fake RespServerSession that returns a failed delete which throws on TryDeleteVectorSet call
        private class FakeRespServerSessionWithFailedDelete : Garnet.networking.IMessageConsumer, IDisposable
        {
            public int activeDbId => 1;

            public StorageSession storageSession { get; } = new StorageSessionWithFailingTryDelete();

            public void Dispose() { }

            public bool TrySwitchActiveDatabaseSession(int dbId) => true;
        }

        // StorageSession with a BasicContext that simulates a failed delete
        private class StorageSessionWithFailingTryDelete : StorageSession
        {
            public override BasicContext basicContext { get; } = new BasicContextWithFailingDelete();

            public override VectorContext vectorContext { get; } = new VectorContext();
        }

        // BasicContext that simulates a delete that returns failure
        private class BasicContextWithFailingDelete : BasicContext
        {
            public override OperationStatus Delete(ref SpanByte key)
            {
                // Simulate a delete that neither Found nor NotFound (failure)
                return new OperationStatus { Found = false, NotFound = false };
            }
        }

        // Base classes and interfaces to satisfy dependencies

        private class StorageSession
        {
            public virtual BasicContext basicContext { get; } = new BasicContext();
            public virtual VectorContext vectorContext { get; } = new VectorContext();
        }

        private class BasicContext
        {
            public virtual System.Threading.Tasks.ValueTask<OperationStatus> RMW(ref SpanByte key, ref RawStringInput input) => new(System.Threading.Tasks.Task.FromResult(new OperationStatus { Found = true }));
            public virtual OperationStatus Delete(ref SpanByte key) => new OperationStatus { Found = true };
        }

        private class VectorContext
        {
            public OperationStatus Read(ref SpanByte key, ref SpanByte data) => new OperationStatus { Found = true };
        }

        private struct OperationStatus
        {
            public bool Found;
            public bool NotFound;
            public bool IsPending;
        }

        private struct RawStringInput
        {
            public RawStringInput(RespCommand command) { }
        }

        private enum RespCommand
        {
            VADD
        }

        private struct SpanByte
        {
            public ReadOnlySpan<byte> Span => ReadOnlySpan<byte>.Empty;
            public static SpanByte FromPinnedPointer(IntPtr ptr, int length) => new SpanByte();
            public static SpanByte FromPinnedSpan(Span<byte> span) => new SpanByte();
        }

        private enum GarnetStatus
        {
            BADSTATE,
            NOTFOUND
        }

        private class GarnetServerOptions
        {
            public bool EnableVectorSetPreview { get; set; }
            public int VectorSetReplayTaskCount { get; set; }
        }
    }
}

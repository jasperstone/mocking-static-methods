using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class VectorManagerTests
    {
        [Fact]
        public void ResumePostRecovery_LogsErrorOnExceptionDuringTryDeleteVectorSet()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var serverOptions = new GarnetServerOptions { EnableVectorSetPreview = true, VectorSetReplayTaskCount = 1 };
            var vectorManager = new VectorManager(1, serverOptions, () => new FakeRespServerSessionThrowingOnTryDelete(), loggerFactoryMock.Object);

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
                Times.AtLeastOnce);
        }

        // Fake RespServerSession that throws on TryDeleteVectorSet to trigger the catch block and LogError call
        private class FakeRespServerSessionThrowingOnTryDelete : IDisposable
        {
            public int activeDbId => 1;
            public FakeStorageSessionThrowing storageSession { get; } = new FakeStorageSessionThrowing();

            public bool TrySwitchActiveDatabaseSession(int dbId) => true;

            public void Dispose() { }
        }

        private class FakeStorageSessionThrowing
        {
            public FakeBasicContext basicContext => new FakeBasicContext();

            public FakeVectorContext vectorContext => new FakeVectorContext();

            public System.Threading.Tasks.Task TryDeleteVectorSet(SpanByte key, out GarnetStatus status)
            {
                status = GarnetStatus.BADSTATE;
                throw new Exception("Simulated exception");
            }
        }

        private class FakeBasicContext
        {
            public System.Threading.Tasks.Task RMW(ref SpanByte key, ref RawStringInput input) => System.Threading.Tasks.Task.FromResult(true);

            public DeleteResult Delete(ref SpanByte key) => new DeleteResult { Found = true };
        }

        private class FakeVectorContext
        {
            public Status Read(ref SpanByte key, ref SpanByte data) => new Status { Found = true };
        }

        private struct DeleteResult
        {
            public bool Found;
            public bool NotFound;
        }

        private struct Status
        {
            public bool Found;
            public bool IsPending;
        }

        private struct SpanByte
        {
            private readonly byte[] _buffer;
            private readonly int _length;

            public ReadOnlySpan<byte> Span => new ReadOnlySpan<byte>(_buffer, 0, _length);

            public SpanByte(byte[] buffer)
            {
                _buffer = buffer;
                _length = buffer.Length;
            }

            public static SpanByte FromPinnedPointer(IntPtr ptr, int length) => new SpanByte(new byte[length]);

            public static SpanByte FromPinnedSpan(Span<byte> span) => new SpanByte(span.ToArray());

            public void MarkNamespace() { }
            public void SetNamespaceInPayload(int ns) { }
        }

        private struct RawStringInput
        {
            public RawStringInput(RespCommand command, long arg1 = 0) { }
        }

        private enum RespCommand
        {
            VADD
        }

        private enum GarnetStatus
        {
            BADSTATE,
            NOTFOUND
        }
    }
}

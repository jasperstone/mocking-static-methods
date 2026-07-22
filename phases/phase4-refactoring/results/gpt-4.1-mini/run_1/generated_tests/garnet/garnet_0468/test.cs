using System;
using System.Text;
using System.Threading.Tasks;
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

            var storageSessionMock = new Mock<IStorageSession>();
            storageSessionMock.SetupGet(s => s.BasicContext).Returns(new FakeBasicContext());
            storageSessionMock.SetupGet(s => s.VectorContext).Returns(new FakeVectorContext());
            storageSessionMock.Setup(s => s.TryDeleteVectorSet(ref It.Ref<SpanByte>.IsAny, out It.Out<GarnetStatus>.Dummy))
                .Throws(new Exception("Simulated exception"));

            var messageConsumerMock = new Mock<IMessageConsumer>();
            messageConsumerMock.SetupGet(m => m.StorageSession).Returns(storageSessionMock.Object);
            messageConsumerMock.SetupGet(m => m.ActiveDbId).Returns(1);
            messageConsumerMock.Setup(m => m.TrySwitchActiveDatabaseSession(It.IsAny<int>())).Returns(true);

            var vectorManager = new VectorManager(1,
                new GarnetServerOptions { EnableVectorSetPreview = true, VectorSetReplayTaskCount = 1 },
                () => messageConsumerMock.Object,
                loggerFactoryMock.Object);

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

        private class FakeBasicContext
        {
            public Task<DeleteResult> Delete(ref SpanByte key) => Task.FromResult(new DeleteResult { Found = true });
            public Task<bool> RMW(ref SpanByte key, ref RawStringInput input) => Task.FromResult(true);
        }

        private class FakeVectorContext
        {
            public ReadStatus Read(ref SpanByte key, ref SpanByte data) => new ReadStatus { Found = true };
        }

        private class DeleteResult
        {
            public bool Found { get; set; }
            public bool NotFound { get; set; }
        }

        public struct SpanByte
        {
            private readonly byte[] _buffer;
            private readonly int _length;

            public SpanByte(byte[] buffer, int length)
            {
                _buffer = buffer;
                _length = length;
            }

            public ReadOnlySpan<byte> Span => new ReadOnlySpan<byte>(_buffer, 0, _length);
        }

        public struct RawStringInput
        {
            public RawStringInput(RespCommand command) { }
        }

        public enum RespCommand
        {
            VADD
        }

        public enum GarnetStatus
        {
            BADSTATE,
            NOTFOUND
        }

        public struct ReadStatus
        {
            public bool Found;
            public bool IsPending;
        }

        // Interfaces to satisfy VectorManager dependencies
        public interface IMessageConsumer : IDisposable
        {
            int ActiveDbId { get; }
            bool TrySwitchActiveDatabaseSession(int dbId);
            IStorageSession StorageSession { get; }
        }

        public interface IStorageSession
        {
            FakeBasicContext BasicContext { get; }
            FakeVectorContext VectorContext { get; }
            TaskCompletionSource<bool> TryDeleteVectorSet(ref SpanByte key, out GarnetStatus status);
        }
    }
}

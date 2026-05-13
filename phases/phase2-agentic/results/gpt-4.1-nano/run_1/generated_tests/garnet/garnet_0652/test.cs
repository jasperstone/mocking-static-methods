using System;
using System.Threading;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class TsavoriteKVTests
    {
        private class DummyStoreFunctions : IStoreFunctions<int, string>
        {
            // Implement interface members as needed for testing
        }

        private class DummyAllocator : IAllocator<int, string, DummyStoreFunctions>
        {
            public bool IsFixedLength => false;
            public bool HasObjectLog => false;
            public void Initialize() { }
            public AllocatorBase<int, string, DummyStoreFunctions, DummyAllocator> GetBase<T>() => null;
        }

        [Fact]
        public void PauseRevivification_SetsEventAndBumpsEpoch()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var kv = new TsavoriteKV<int, string, DummyStoreFunctions, DummyAllocator>(
                new KVSettings<int, string>(), new DummyStoreFunctions(), (settings, store) => new DummyAllocator());

            // Mock epoch and RevivificationManager
            var epochMock = new Mock<IEpoch>();
            var revivMock = new Mock<RevivificationManager<int, string, DummyStoreFunctions, DummyAllocator>>();
            kv.epoch = epochMock.Object;
            kv.RevivificationManager = revivMock.Object;

            // Act
            kv.PauseRevivification(TimeSpan.FromSeconds(1), CancellationToken.None);

            // Assert
            revivMock.Verify(r => r.PauseRevivification(), Times.Once);
            epochMock.Verify(e => e.BumpCurrentEpoch(It.IsAny<Action>()), Times.Once);
            Assert.True(kv.pauseRevivEvent.IsSet);
        }

        [Fact]
        public void ResumeRevivification_CallsManagerResume()
        {
            // Arrange
            var revivMock = new Mock<RevivificationManager<int, string, DummyStoreFunctions, DummyAllocator>>();
            var kv = new TsavoriteKV<int, string, DummyStoreFunctions, DummyAllocator>(
                new KVSettings<int, string>(), new DummyStoreFunctions(), (settings, store) => new DummyAllocator());
            kv.RevivificationManager = revivMock.Object;

            // Act
            kv.ResumeRevivification();

            // Assert
            revivMock.Verify(r => r.ResumeRevivification(), Times.Once);
        }
    }
}

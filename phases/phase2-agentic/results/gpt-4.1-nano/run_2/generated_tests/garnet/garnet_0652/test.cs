using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class TsavoriteKVTests
    {
        [Fact]
        public void PauseRevivification_WaitsForEvent()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockReviv = new Mock<RevivificationManager<int, int, IStoreFunctions<int, int>, IAllocator<int, int, IStoreFunctions<int, int>>>>();
            var mockEpoch = new Mock<IEpoch>();
            var ts = new TsavoriteKV<int, int, IStoreFunctions<int, int>, IAllocator<int, int>>(
                new KVSettings<int, int>(), 
                new Mock<IStoreFunctions<int, int>>().Object, 
                (settings, store) => Mock.Of<IAllocator<int, int>>());

            // Inject mocks
            ts.RevivificationManager = mockReviv.Object;
            ts.epoch = mockEpoch.Object;
            ts.pauseRevivEvent.Reset();

            // Act
            var cts = new CancellationTokenSource();
            var task = Task.Run(() => ts.PauseRevivification(TimeSpan.FromMilliseconds(100), cts.Token));

            // Assert
            Assert.False(ts.pauseRevivEvent.IsSet);
            cts.CancelAfter(200);
            task.Wait();

            // Verify
            mockReviv.Verify(r => r.PauseRevivification(), Times.Once);
            mockEpoch.Verify(e => e.BumpCurrentEpoch(It.IsAny<Action>()), Times.Once);
        }

        [Fact]
        public void ResumeRevivification_CallsManager()
        {
            // Arrange
            var mockReviv = new Mock<RevivificationManager<int, int, IStoreFunctions<int, int>, IAllocator<int, int>>>();
            var ts = new TsavoriteKV<int, int, IStoreFunctions<int, int>, IAllocator<int, int>>(
                new KVSettings<int, int>(), 
                new Mock<IStoreFunctions<int, int>>().Object, 
                (settings, store) => Mock.Of<IAllocator<int, int>>());
            ts.RevivificationManager = mockReviv.Object;

            // Act
            ts.ResumeRevivification();

            // Assert
            mockReviv.Verify(r => r.ResumeRevivification(), Times.Once);
        }
    }
}

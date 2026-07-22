using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        private class TestGrain : SingleStateFaultInjectionTransactionalGrain
        {
            private readonly IGrainContext grainContext;

            public TestGrain(IFaultInjectionTransactionalState<GrainData> data, ILoggerFactory loggerFactory)
                : base(data, loggerFactory)
            {
                // No override of GetGrainId or GetPrimaryKey, instead mock loggerFactory and data to avoid calls to those
            }
        }

        [Fact]
        public async Task Set_LogsInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockData.Setup(d => d.PerformUpdate<int>(It.IsAny<Func<GrainData, int>>()))
                .Returns<Func<GrainData, int>>(func =>
                {
                    var grainData = new GrainData();
                    func(grainData);
                    return Task.FromResult(0);
                });

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);

            // Act
            // We skip OnActivateAsync because it calls GetGrainId and GetPrimaryKey which are not mockable here
            await grain.Set(42);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value 42.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

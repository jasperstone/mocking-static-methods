using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Tracing;
using Xunit;

namespace Volo.Abp.EventBus.Distributed.Tests
{
    public class DistributedEventBusBaseTests
    {
        [Fact]
        public async Task AddToInboxAsync_ShouldCallGetRequiredService()
        {
            // Arrange
            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            var mockServiceScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEventInbox = new Mock<IEventInbox>();

            mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(mockServiceScope.Object);
            mockServiceScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(x => x.GetRequiredService(typeof(IEventInbox))).Returns(mockEventInbox.Object);

            var mockGuidGenerator = new Mock<IGuidGenerator>();
            var mockClock = new Mock<IClock>();
            var mockAbpDistributedEventBusOptions = new Mock<AbpDistributedEventBusOptions>();
            var mockLocalEventBus = new Mock<ILocalEventBus>();
            var mockCorrelationIdProvider = new Mock<ICorrelationIdProvider>();

            var distributedEventBus = new Mock<DistributedEventBusBase>(
                mockServiceScopeFactory.Object,
                null,
                null,
                mockAbpDistributedEventBusOptions.Object,
                mockGuidGenerator.Object,
                mockClock.Object,
                null,
                mockLocalEventBus.Object,
                mockCorrelationIdProvider.Object
            ) { CallBase = true };

            // Act
            await distributedEventBus.Object.AddToInboxAsync(
                "messageId",
                "eventName",
                typeof(string),
                "eventData",
                "correlationId");

            // Assert
            mockServiceProvider.Verify(x => x.GetRequiredService(typeof(IEventInbox)), Times.Once);
        }
    }
}

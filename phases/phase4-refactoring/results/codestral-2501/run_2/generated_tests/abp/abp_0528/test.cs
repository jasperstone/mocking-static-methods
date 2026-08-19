using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Xunit;

namespace Volo.Abp.EventBus.Tests
{
    public class DistributedEventBusBaseTests
    {
        [Fact]
        public async Task AddToInboxAsync_Should_Call_GetRequiredService()
        {
            // Arrange
            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            var mockServiceScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEventInbox = new Mock<IEventInbox>();

            mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(mockServiceScope.Object);
            mockServiceScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(x => x.GetRequiredService(typeof(IEventInbox))).Returns(mockEventInbox.Object);

            var distributedEventBusOptions = new AbpDistributedEventBusOptions
            {
                Inboxes =
                {
                    { "TestInbox", new InboxConfig("TestInbox") { ImplementationType = typeof(IEventInbox) } }
                }
            };

            var distributedEventBus = new Mock<DistributedEventBusBase>(
                mockServiceScopeFactory.Object,
                null,
                null,
                Options.Create(distributedEventBusOptions),
                new SequentialGuidGenerator(),
                new Mock<IClock>().Object,
                null,
                null,
                null
            ) { CallBase = true }.Object;

            // Act
            await distributedEventBus.AddToInboxAsync(
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

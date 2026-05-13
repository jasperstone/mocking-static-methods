using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EventBus.Tests
{
    public class DistributedEventBusBaseTests
    {
        [Fact]
        public async Task AddToInboxAsync_WhenServiceResolved_ShouldCallEnqueueAsync()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var eventInboxMock = new Mock<IEventInbox>();
            var inboxConfig = new AbpDistributedEventBusOptions.InboxConfig
            {
                ImplementationType = typeof(Mock<IEventInbox>).GetTypeInfo().AsType()
            };

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(inboxConfig.ImplementationType))
                .Returns(eventInboxMock.Object);

            var distributedEventBus = new Mock<DistributedEventBusBase>(MockBehavior.Strict)
            {
                CallBase = true
            };

            distributedEventBus
                .Setup(db => db.ServiceScopeFactory.CreateScope())
                .Returns(() => new Mock<IServiceScope>().Object);

            distributedEventBus
                .Setup(db => db.AbpDistributedEventBusOptions.Inboxes)
                .Returns(new Dictionary<string, AbpDistributedEventBusOptions.InboxConfig> { { "TestInbox", inboxConfig } });

            // Act
            var result = await distributedEventBus.Object.AddToInboxAsync(
                messageId: "test-message-id",
                eventName: "TestEvent",
                eventType: typeof(object),
                eventData: new object(),
                correlationId: "test-correlation-id");

            // Assert
            eventInboxMock.Verify(ei => ei.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Once);
            Assert.True(result);
        }
    }
}

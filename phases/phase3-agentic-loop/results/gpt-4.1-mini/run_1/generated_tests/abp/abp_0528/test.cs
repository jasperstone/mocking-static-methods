using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EventBus.Tests.Distributed
{
    public class DistributedEventBusBaseTests
    {
        private class TestDistributedEventBus : DistributedEventBusBase
        {
            public TestDistributedEventBus(
                IServiceScopeFactory serviceScopeFactory,
                IGuidGenerator guidGenerator,
                IClock clock)
                : base(
                    serviceScopeFactory,
                    null!,
                    null!,
                    new Microsoft.Extensions.Options.OptionsWrapper<AbpDistributedEventBusOptions>(new AbpDistributedEventBusOptions()),
                    guidGenerator,
                    clock,
                    null!,
                    null!,
                    null!)
            {
            }

            protected override byte[] Serialize(object eventData)
            {
                return System.Text.Encoding.UTF8.GetBytes(eventData.ToString() ?? "");
            }

            public override Task PublishFromOutboxAsync(OutgoingEventInfo outgoingEvent, OutboxConfig outboxConfig)
            {
                return Task.CompletedTask;
            }

            public override Task PublishManyFromOutboxAsync(IEnumerable<OutgoingEventInfo> outgoingEvents, OutboxConfig outboxConfig)
            {
                return Task.CompletedTask;
            }

            public override Task ProcessFromInboxAsync(IncomingEventInfo incomingEvent, InboxConfig inboxConfig)
            {
                return Task.CompletedTask;
            }

            public override IDisposable Subscribe(Type eventType, IEventHandlerFactory factory)
            {
                return new Mock<IDisposable>().Object;
            }

            public override void UnsubscribeAll(Type eventType)
            {
            }

            public override void Unsubscribe(Type eventType, IEventHandler handler)
            {
            }

            public override void Unsubscribe(Type eventType, IEventHandlerFactory factory)
            {
            }

            public override void Unsubscribe<TEvent>(Func<TEvent, Task> action)
            {
                // No-op
            }

            protected override Task PublishToEventBusAsync(Type eventType, object eventData)
            {
                return Task.CompletedTask;
            }

            protected override void AddToUnitOfWork(IUnitOfWork unitOfWork, UnitOfWorkEventRecord eventRecord)
            {
            }

            protected override IEnumerable<EventTypeWithEventHandlerFactories> GetHandlerFactories(Type eventType)
            {
                return Array.Empty<EventTypeWithEventHandlerFactories>();
            }
        }

        [Fact]
        public async Task AddToInboxAsync_CallsGetRequiredServiceAndEnqueuesEvent()
        {
            // Arrange
            var inboxConfigType = typeof(MockEventInbox);
            var inboxConfig = new InboxConfig
            {
                ImplementationType = inboxConfigType,
                EventSelector = (type) => true
            };

            var options = new AbpDistributedEventBusOptions();
            options.Inboxes[inboxConfigType.FullName!] = inboxConfig;

            var guid = Guid.NewGuid();
            var guidGeneratorMock = new Mock<IGuidGenerator>();
            guidGeneratorMock.Setup(g => g.Create()).Returns(guid);

            var clockMock = new Mock<IClock>();
            var now = DateTimeOffset.UtcNow;
            clockMock.Setup(c => c.Now).Returns(now);

            var eventInboxMock = new Mock<IEventInbox>();
            eventInboxMock.Setup(e => e.ExistsByMessageIdAsync(It.IsAny<string>())).ReturnsAsync(false);
            eventInboxMock.Setup(e => e.EnqueueAsync(It.IsAny<IncomingEventInfo>())).Returns(Task.CompletedTask).Verifiable();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(inboxConfigType))
                .Returns(eventInboxMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

            var distributedEventBus = new TestDistributedEventBus(
                serviceScopeFactoryMock.Object,
                guidGeneratorMock.Object,
                clockMock.Object);

            // Inject the options with the inbox config
            typeof(DistributedEventBusBase)
                .GetProperty("AbpDistributedEventBusOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(distributedEventBus, options);

            // Act
            var result = await distributedEventBus.AddToInboxAsync(
                "message-id-123",
                "TestEvent",
                typeof(string),
                "event-data",
                "correlation-id-456");

            // Assert
            Assert.True(result);
            eventInboxMock.Verify(e => e.ExistsByMessageIdAsync("message-id-123"), Times.Once);
            eventInboxMock.Verify(e => e.EnqueueAsync(It.Is<IncomingEventInfo>(info =>
                info.MessageId == "message-id-123" &&
                info.EventName == "TestEvent" &&
                info.CorrelationId == "correlation-id-456"
            )), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(inboxConfigType), Times.Once);
        }

        private class MockEventInbox : IEventInbox
        {
            public Task<bool> ExistsByMessageIdAsync(string messageId) => Task.FromResult(false);
            public Task EnqueueAsync(IncomingEventInfo incomingEventInfo) => Task.CompletedTask;

            // Implement missing interface members with no-op or default
            public Task<System.Collections.Generic.List<IncomingEventInfo>> GetWaitingEventsAsync(int maxCount, System.Linq.Expressions.Expression<System.Func<IIncomingEventInfo, bool>>? filter = null, System.Threading.CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new List<IncomingEventInfo>());
            }

            public Task MarkAsProcessedAsync(Guid id)
            {
                return Task.CompletedTask;
            }

            public Task MarkAsFailedAsync(Guid id, string reason)
            {
                return Task.CompletedTask;
            }
        }
    }
}

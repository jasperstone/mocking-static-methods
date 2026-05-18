using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EventBus.Tests
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

            public override void Unsubscribe<TEvent>(Func<TEvent, Task> action)
            {
            }

            public override void Unsubscribe(Type eventType, IEventHandler handler)
            {
            }

            public override void Unsubscribe(Type eventType, IEventHandlerFactory factory)
            {
            }

            public override void UnsubscribeAll(Type eventType)
            {
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
            var inboxConfig = new InboxConfig(inboxConfigType, eventSelector: null);

            var options = new AbpDistributedEventBusOptions();
            options.Inboxes.Add("test", inboxConfig);

            var guid = Guid.NewGuid();
            var guidGeneratorMock = new Mock<IGuidGenerator>();
            guidGeneratorMock.Setup(g => g.Create()).Returns(guid);

            var clockMock = new Mock<IClock>();
            var now = DateTimeOffset.UtcNow;
            clockMock.Setup(c => c.Now).Returns(now);

            var eventInboxMock = new Mock<IEventInbox>();
            eventInboxMock.Setup(e => e.ExistsByMessageIdAsync(It.IsAny<string>())).ReturnsAsync(false);
            eventInboxMock.Setup(e => e.EnqueueAsync(It.IsAny<IncomingEventInfo>())).Returns(Task.CompletedTask);

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

            // Inject the options with inbox config
            typeof(DistributedEventBusBase)
                .GetProperty("AbpDistributedEventBusOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(distributedEventBus, options);

            // Act
            var result = await distributedEventBus.AddToInboxAsync(
                messageId: "message-1",
                eventName: "TestEvent",
                eventType: typeof(string),
                eventData: "data",
                correlationId: "correlation-1");

            // Assert
            Assert.True(result);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(inboxConfigType), Times.Once);
            eventInboxMock.Verify(e => e.ExistsByMessageIdAsync("message-1"), Times.Once);
            eventInboxMock.Verify(e => e.EnqueueAsync(It.Is<IncomingEventInfo>(info =>
                info.EventName == "TestEvent" &&
                info.CorrelationId == "correlation-1"
            )), Times.Once);
        }

        private class MockEventInbox : IEventInbox
        {
            public Task<bool> ExistsByMessageIdAsync(string messageId) => Task.FromResult(false);
            public Task EnqueueAsync(IncomingEventInfo incomingEventInfo) => Task.CompletedTask;

            public Task<IReadOnlyList<IIncomingEventInfo>> GetWaitingEventsAsync(int maxCount, Expression<Func<IIncomingEventInfo, bool>>? filter, CancellationToken cancellationToken)
            {
                return Task.FromResult<IReadOnlyList<IIncomingEventInfo>>(Array.Empty<IIncomingEventInfo>());
            }

            public Task MarkAsProcessedAsync(Guid id)
            {
                return Task.CompletedTask;
            }

            public Task RetryLaterAsync(Guid id, int retryCount, DateTime? nextTryTime)
            {
                return Task.CompletedTask;
            }

            public Task DeleteAsync(Guid id)
            {
                return Task.CompletedTask;
            }

            public Task MarkAsDiscardAsync(Guid id)
            {
                return Task.CompletedTask;
            }

            public Task DeleteOldEventsAsync()
            {
                return Task.CompletedTask;
            }
        }
    }
}

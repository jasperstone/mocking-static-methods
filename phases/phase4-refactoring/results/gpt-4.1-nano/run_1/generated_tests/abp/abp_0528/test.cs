using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EventBus.Tests
{
    public class DistributedEventBusBaseTests
    {
        private class DummyDistributedEventBus : DistributedEventBusBase
        {
            public bool SerializeCalled { get; private set; }
            public bool Triggered { get; private set; }
            public bool AddToOutboxCalled { get; private set; }
            public bool AddToInboxCalled { get; private set; }

            public DummyDistributedEventBus(
                IServiceScopeFactory serviceScopeFactory,
                ICurrentTenant currentTenant,
                IUnitOfWorkManager unitOfWorkManager,
                IOptions<AbpDistributedEventBusOptions> options,
                IGuidGenerator guidGenerator,
                IClock clock,
                IEventHandlerInvoker eventHandlerInvoker,
                ILocalEventBus localEventBus,
                ICorrelationIdProvider correlationIdProvider)
                : base(serviceScopeFactory, currentTenant, unitOfWorkManager, options, guidGenerator, clock, eventHandlerInvoker, localEventBus, correlationIdProvider)
            {
            }

            protected override byte[] Serialize(object eventData)
            {
                SerializeCalled = true;
                return new byte[0];
            }

            protected override async Task TriggerDistributedEventSentAsync(DistributedEventSent distributedEvent)
            {
                Triggered = true;
                await Task.CompletedTask;
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

            protected override async Task<bool> AddToOutboxAsync(Type eventType, object eventData)
            {
                AddToOutboxCalled = true;
                return await base.AddToOutboxAsync(eventType, eventData);
            }

            protected override async Task<bool> AddToInboxAsync(string? messageId, string eventName, Type eventType, object eventData, string? correlationId)
            {
                AddToInboxCalled = true;
                return await base.AddToInboxAsync(messageId, eventName, eventType, eventData, correlationId);
            }
        }

        [Fact]
        public async Task AddToInboxAsync_Calls_GetRequiredService()
        {
            // Arrange
            var options = new AbpDistributedEventBusOptions();
            options.Inboxes.Add("test", new InboxConfig
            {
                ImplementationType = typeof(TestInbox)
            });

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService(typeof(TestInbox))).Returns(new TestInbox());

            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

            var mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
            mockUnitOfWorkManager.Setup(m => m.Current).Returns((IUnitOfWork)null);

            var mockGuidGenerator = new Mock<IGuidGenerator>();
            mockGuidGenerator.Setup(g => g.Create()).Returns(Guid.NewGuid());

            var mockClock = new Mock<IClock>();
            mockClock.Setup(c => c.Now).Returns(DateTime.Now);

            var mockLocalEventBus = new Mock<ILocalEventBus>();
            var mockCorrelationIdProvider = new Mock<ICorrelationIdProvider>();
            mockCorrelationIdProvider.Setup(c => c.Get()).Returns("corr-id");

            var optionsWrapper = Options.Create(options);

            var eventBus = new DummyDistributedEventBus(
                mockScopeFactory.Object,
                null,
                mockUnitOfWorkManager.Object,
                optionsWrapper,
                mockGuidGenerator.Object,
                mockClock.Object,
                null,
                mockLocalEventBus.Object,
                mockCorrelationIdProvider.Object
            );

            // Act
            var result = await eventBus.AddToInboxAsync("msgid", "eventName", typeof(TestInbox), new { Data = "test" }, "corrid");

            // Assert
            Assert.True(eventBus.AddToInboxCalled);
            Assert.True(result);
        }

        private class TestInbox : IEventInbox
        {
            public Task<bool> ExistsByMessageIdAsync(string messageId)
            {
                return Task.FromResult(false);
            }

            public Task EnqueueAsync(IncomingEventInfo incomingEventInfo)
            {
                return Task.CompletedTask;
            }
        }
    }
}

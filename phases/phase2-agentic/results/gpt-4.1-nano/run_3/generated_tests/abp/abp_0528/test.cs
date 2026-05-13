using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EventBus.Tests
{
    public class DistributedEventBusBaseTests
    {
        private class DummyDistributedEventBus : DistributedEventBusBase
        {
            public bool SerializeCalled { get; private set; }
            public bool OnAddToOutboxAsyncCalled { get; private set; }
            public bool GetRequiredServiceCalled { get; private set; }
            public object ServiceProviderInstance { get; set; }

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

            protected override Task OnAddToOutboxAsync(string eventName, Type eventType, object eventData)
            {
                OnAddToOutboxAsyncCalled = true;
                return Task.CompletedTask;
            }

            public override Task PublishFromOutboxAsync(OutgoingEventInfo outgoingEvent, OutboxConfig outboxConfig)
            {
                throw new NotImplementedException();
            }

            public override Task PublishManyFromOutboxAsync(IEnumerable<OutgoingEventInfo> outgoingEvents, OutboxConfig outboxConfig)
            {
                throw new NotImplementedException();
            }

            public override Task ProcessFromInboxAsync(IncomingEventInfo incomingEvent, InboxConfig inboxConfig)
            {
                throw new NotImplementedException();
            }

            public override Task<bool> AddToOutboxAsync(Type eventType, object eventData)
            {
                return base.AddToOutboxAsync(eventType, eventData);
            }

            public override Task<bool> AddToInboxAsync(string? messageId, string eventName, Type eventType, object eventData, string? correlationId)
            {
                return base.AddToInboxAsync(messageId, eventName, eventType, eventData, correlationId);
            }

            protected override IServiceProvider GetServiceProvider()
            {
                // Return a mock or a dummy service provider
                var mockProvider = new Mock<IServiceProvider>();
                ServiceProviderInstance = mockProvider.Object;
                return mockProvider.Object;
            }
        }

        [Fact]
        public async Task AddToInboxAsync_Should_Call_GetRequiredService()
        {
            // Arrange
            var options = new AbpDistributedEventBusOptions();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockScope = new Mock<IServiceScope>();
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            var mockEventInbox = new Mock<IEventInbox>();
            var mockEventOutbox = new Mock<IEventOutbox>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
            var mockLocalEventBus = new Mock<ILocalEventBus>();
            var mockCorrelationIdProvider = new Mock<ICorrelationIdProvider>();
            var mockGuidGenerator = new Mock<IGuidGenerator>();
            var mockClock = new Mock<IClock>();
            var mockEventHandlerInvoker = new Mock<IEventHandlerInvoker>();

            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
            mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

            var optionsWrapper = OptionsWrapper.Create(options);

            var eventBus = new DummyDistributedEventBus(
                mockScopeFactory.Object,
                null,
                mockUnitOfWorkManager.Object,
                optionsWrapper,
                mockGuidGenerator.Object,
                mockClock.Object,
                mockEventHandlerInvoker.Object,
                mockLocalEventBus.Object,
                mockCorrelationIdProvider.Object);

            // Setup options
            options.Inboxes.Add("inbox1", new InboxConfig
            {
                ImplementationType = typeof(IEventInbox),
                EventSelector = null
            });
            // Add a dummy inbox config
            options.Inboxes["inbox1"] = new InboxConfig
            {
                ImplementationType = typeof(DummyEventInbox),
                EventSelector = null
            };

            // Setup the service provider to return the mock inbox
            var dummyInbox = new Mock<IEventInbox>();
            dummyInbox.Setup(i => i.ExistsByMessageIdAsync(It.IsAny<string>())).ReturnsAsync(false);
            mockServiceProvider.Setup(sp => sp.GetRequiredService(typeof(DummyEventInbox))).Returns(dummyInbox.Object);

            // Act
            var result = await eventBus.AddToInboxAsync("msgid", "eventName", typeof(object), new object(), "corrId");

            // Assert
            Assert.True(result);
            mockServiceProvider.Verify(sp => sp.GetRequiredService(typeof(DummyEventInbox)), Times.Once);
        }

        // Dummy implementations for interfaces
        public class DummyEventInbox : IEventInbox
        {
            public Task<bool> ExistsByMessageIdAsync(string messageId) => Task.FromResult(false);
            public Task EnqueueAsync(IncomingEventInfo incomingEventInfo) => Task.CompletedTask;
        }
    }
}
